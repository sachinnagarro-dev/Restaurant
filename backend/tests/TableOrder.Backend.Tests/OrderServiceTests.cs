using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.SignalR;
using Xunit;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Models;
using TableOrder.Backend.Services;
using TableOrder.Backend.Hubs;

namespace TableOrder.Backend.Tests;

public class OrderServiceTests : IDisposable
{
    private readonly TableOrderDbContext _context;
    private readonly IHubContext<Hubs.OrderHub> _hubContext;
    private readonly OrderService _orderService;

    public OrderServiceTests()
    {
        var options = new DbContextOptionsBuilder<TableOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TableOrderDbContext(options);
        _hubContext = new MockHubContext<Hubs.OrderHub>();
        _orderService = new OrderService(_context, _hubContext);

        // Seed test data
        SeedTestData();
    }

    [Fact]
    public async Task CreateOrder_ValidRequest_ReturnsOrderResponse()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            TableId = 1,
            OrderItems = new List<OrderItemRequest>
            {
                new OrderItemRequest
                {
                    MenuItemId = 1,
                    Quantity = 2,
                    SpecialInstructions = "Extra cheese"
                }
            },
            SpecialInstructions = "Please hurry"
        };

        // Act
        var result = await _orderService.CreateOrderAsync(request);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(1, result.TableId);
        Assert.Equal(1, result.TableNumber);
        Assert.Equal("Received", result.Status);
        Assert.Equal("Please hurry", result.SpecialInstructions);
        Assert.Single(result.OrderItems);
        Assert.Equal("Test Pizza", result.OrderItems.First().MenuItemName);
        Assert.Equal(2, result.OrderItems.First().Quantity);
        Assert.Equal("Extra cheese", result.OrderItems.First().SpecialInstructions);
    }

    [Fact]
    public async Task CreateOrder_InvalidTableId_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            TableId = 999, // Non-existent table
            OrderItems = new List<OrderItemRequest>
            {
                new OrderItemRequest
                {
                    MenuItemId = 1,
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrderAsync(request));
    }

    [Fact]
    public async Task CreateOrder_InvalidMenuItemId_ThrowsArgumentException()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            TableId = 1,
            OrderItems = new List<OrderItemRequest>
            {
                new OrderItemRequest
                {
                    MenuItemId = 999, // Non-existent menu item
                    Quantity = 1
                }
            }
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _orderService.CreateOrderAsync(request));
    }

    [Fact]
    public async Task UpdateOrderStatus_ValidId_ReturnsUpdatedOrder()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            TableId = 1,
            OrderItems = new List<OrderItemRequest>
            {
                new OrderItemRequest
                {
                    MenuItemId = 1,
                    Quantity = 1
                }
            }
        };

        var order = await _orderService.CreateOrderAsync(request);

        // Act
        var result = await _orderService.UpdateOrderStatusAsync(order.Id, OrderStatus.Preparing);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Preparing", result.Status);
    }

    [Fact]
    public async Task UpdateOrderStatus_InvalidId_ReturnsNull()
    {
        // Act
        var result = await _orderService.UpdateOrderStatusAsync(999, OrderStatus.Preparing);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetOrdersByTable_ValidTableId_ReturnsOrders()
    {
        // Arrange
        var request = new CreateOrderRequest
        {
            TableId = 1,
            OrderItems = new List<OrderItemRequest>
            {
                new OrderItemRequest
                {
                    MenuItemId = 1,
                    Quantity = 1
                }
            }
        };

        await _orderService.CreateOrderAsync(request);

        // Act
        var result = await _orderService.GetOrdersByTableAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Single(result);
        Assert.Equal(1, result.First().TableId);
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

