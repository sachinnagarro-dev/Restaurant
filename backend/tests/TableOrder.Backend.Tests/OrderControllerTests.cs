using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Microsoft.AspNetCore.SignalR;
using Xunit;
using TableOrder.Backend.Controllers;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Models;
using TableOrder.Backend.Hubs;

namespace TableOrder.Backend.Tests;

public class OrderControllerTests : IDisposable
{
    private readonly TableOrderDbContext _context;
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly ILogger<OrderController> _logger;
    private readonly OrderController _controller;

    public OrderControllerTests()
    {
        var options = new DbContextOptionsBuilder<TableOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TableOrderDbContext(options);
        _hubContext = new MockHubContext<OrderHub>();
        _logger = new MockLogger<OrderController>();
        _controller = new OrderController(_context, _hubContext, _logger);

        SeedTestData();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsCreatedResult()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            TableId = 1,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    MenuItemId = 1,
                    Quantity = 2,
                    SpecialInstructions = "Extra cheese"
                }
            },
            Remarks = "Please hurry"
        };

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        Assert.IsType<CreatedAtActionResult>(result.Result);
        var createdResult = (CreatedAtActionResult)result.Result!;
        Assert.Equal(201, createdResult.StatusCode);
        
        var orderResponse = (OrderResponseDto)createdResult.Value!;
        Assert.Equal(1, orderResponse.TableId);
        Assert.Equal(1, orderResponse.TableNumber);
        Assert.Equal("Received", orderResponse.Status);
        Assert.Equal("Please hurry", orderResponse.Remarks);
        Assert.Single(orderResponse.Items);
        Assert.Equal("Test Pizza", orderResponse.Items.First().MenuItemName);
        Assert.Equal(2, orderResponse.Items.First().Quantity);
        Assert.Equal("Extra cheese", orderResponse.Items.First().SpecialInstructions);
    }

    [Fact]
    public async Task CreateOrder_InvalidTableId_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            TableId = 999, // Non-existent table
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    MenuItemId = 1,
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_InvalidMenuItemId_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            TableId = 1,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    MenuItemId = 999, // Non-existent menu item
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_UnavailableMenuItem_ReturnsBadRequest()
    {
        // Arrange
        // Make menu item unavailable
        var menuItem = await _context.MenuItems.FindAsync(1);
        menuItem!.IsAvailable = false;
        await _context.SaveChangesAsync();

        var request = new CreateOrderDto
        {
            TableId = 1,
            Items = new List<OrderItemDto>
            {
                new OrderItemDto
                {
                    MenuItemId = 1,
                    Quantity = 1
                }
            }
        };

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task CreateOrder_EmptyItems_ReturnsBadRequest()
    {
        // Arrange
        var request = new CreateOrderDto
        {
            TableId = 1,
            Items = new List<OrderItemDto>() // Empty items
        };

        // Act
        var result = await _controller.CreateOrder(request);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task GetOrder_ValidId_ReturnsOrder()
    {
        // Arrange
        var order = await CreateTestOrder();

        // Act
        var result = await _controller.GetOrder(order.Id);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);
        
        var orderResponse = (OrderResponseDto)okResult.Value!;
        Assert.Equal(order.Id, orderResponse.Id);
        Assert.Equal(1, orderResponse.TableId);
        Assert.Equal("Received", orderResponse.Status);
    }

    [Fact]
    public async Task GetOrder_InvalidId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetOrder(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        var notFoundResult = (NotFoundObjectResult)result.Result!;
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task UpdateOrderStatus_ValidId_ReturnsUpdatedOrder()
    {
        // Arrange
        var order = await CreateTestOrder();
        var updateRequest = new UpdateOrderStatusDto
        {
            Status = OrderStatus.Preparing
        };

        // Act
        var result = await _controller.UpdateOrderStatus(order.Id, updateRequest);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);
        
        var orderResponse = (OrderResponseDto)okResult.Value!;
        Assert.Equal("Preparing", orderResponse.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidId_ReturnsNotFound()
    {
        // Arrange
        var updateRequest = new UpdateOrderStatusDto
        {
            Status = OrderStatus.Preparing
        };

        // Act
        var result = await _controller.UpdateOrderStatus(999, updateRequest);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        var notFoundResult = (NotFoundObjectResult)result.Result!;
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetAllOrders_ReturnsAllOrders()
    {
        // Arrange
        await CreateTestOrder();
        await CreateTestOrder();

        // Act
        var result = await _controller.GetAllOrders();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);
        
        var orders = (IEnumerable<OrderSummaryDto>)okResult.Value!;
        Assert.Equal(2, orders.Count());
    }

    [Fact]
    public async Task GetOrdersByTable_ValidTableId_ReturnsOrders()
    {
        // Arrange
        await CreateTestOrder();

        // Act
        var result = await _controller.GetOrdersByTable(1);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);
        
        var orders = (IEnumerable<OrderSummaryDto>)okResult.Value!;
        Assert.Single(orders);
        Assert.Equal(1, orders.First().TableId);
    }

    [Fact]
    public async Task GetOrdersByStatus_ValidStatus_ReturnsOrders()
    {
        // Arrange
        await CreateTestOrder();

        // Act
        var result = await _controller.GetOrdersByStatus("Received");

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);
        
        var orders = (IEnumerable<OrderSummaryDto>)okResult.Value!;
        Assert.Single(orders);
        Assert.Equal("Received", orders.First().Status);
    }

    [Fact]
    public async Task GetOrdersByStatus_InvalidStatus_ReturnsBadRequest()
    {
        // Act
        var result = await _controller.GetOrdersByStatus("InvalidStatus");

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    private async Task<Order> CreateTestOrder()
    {
        var order = new Order
        {
            TableId = 1,
            Status = OrderStatus.Received,
            SubTotal = 25.98m,
            TaxAmount = 2.08m,
            TotalAmount = 28.06m,
            Remarks = "Test order",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var orderItem = new OrderItem
        {
            Order = order,
            MenuItemId = 1,
            Quantity = 2,
            UnitPrice = 12.99m,
            CreatedAt = DateTime.UtcNow
        };

        _context.Orders.Add(order);
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();

        return order;
    }

    private void SeedTestData()
    {
        var restaurant = new Restaurant
        {
            Id = 1,
            Name = "Test Restaurant",
            Address = "123 Test St",
            Phone = "(555) 123-4567",
            Email = "test@restaurant.com"
        };

        var table = new Table
        {
            Id = 1,
            Number = 1,
            Capacity = 4,
            Status = TableStatus.Available,
            RestaurantId = 1
        };

        var menuItem = new MenuItem
        {
            Id = 1,
            Name = "Test Pizza",
            Description = "A delicious test pizza",
            Price = 12.99m,
            Category = "Pizza",
            IsAvailable = true,
            IsVegetarian = true,
            PreparationTimeMinutes = 20,
            RestaurantId = 1
        };

        _context.Restaurants.Add(restaurant);
        _context.Tables.Add(table);
        _context.MenuItems.Add(menuItem);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

// Mock implementations for testing
public class MockHubContext<T> : IHubContext<T> where T : Hub
{
    public IHubClients Clients => new MockHubClients();
    public IGroupManager Groups => new MockGroupManager();
}

public class MockHubClients : IHubClients
{
    public IClientProxy All => new MockClientProxy();
    public IClientProxy AllExcept(IReadOnlyList<string> excludedConnectionIds) => new MockClientProxy();
    public IClientProxy Client(string connectionId) => new MockClientProxy();
    public IClientProxy Clients(IReadOnlyList<string> connectionIds) => new MockClientProxy();
    public IClientProxy Group(string groupName) => new MockClientProxy();
    public IClientProxy GroupExcept(string groupName, IReadOnlyList<string> excludedConnectionIds) => new MockClientProxy();
    public IClientProxy Groups(IReadOnlyList<string> groupNames) => new MockClientProxy();
    public IClientProxy User(string userId) => new MockClientProxy();
    public IClientProxy Users(IReadOnlyList<string> userIds) => new MockClientProxy();
}

public class MockClientProxy : IClientProxy
{
    public Task SendCoreAsync(string method, object?[] args, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class MockGroupManager : IGroupManager
{
    public Task AddToGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }

    public Task RemoveFromGroupAsync(string connectionId, string groupName, CancellationToken cancellationToken = default)
    {
        return Task.CompletedTask;
    }
}

public class MockLogger<T> : ILogger<T>
{
    public IDisposable? BeginScope<TState>(TState state) where TState : notnull => null;
    public bool IsEnabled(LogLevel logLevel) => true;
    public void Log<TState>(LogLevel logLevel, EventId eventId, TState state, Exception? exception, Func<TState, Exception?, string> formatter) { }
}
