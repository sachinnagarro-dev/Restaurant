using Microsoft.EntityFrameworkCore;
using Xunit;
using TableOrder.Backend.Data;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.Tests;

public class TableOrderDbContextTests : IDisposable
{
    private readonly TableOrderDbContext _context;

    public TableOrderDbContextTests()
    {
        var options = new DbContextOptionsBuilder<TableOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TableOrderDbContext(options);
    }

    [Fact]
    public async Task CanCreateAndRetrieveRestaurant()
    {
        // Arrange
        var restaurant = new Restaurant
        {
            Name = "Test Restaurant",
            Address = "123 Test St",
            Phone = "(555) 123-4567",
            Email = "test@restaurant.com"
        };

        // Act
        _context.Restaurants.Add(restaurant);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Restaurants.FirstAsync();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Test Restaurant", retrieved.Name);
        Assert.Equal("123 Test St", retrieved.Address);
        Assert.Equal("(555) 123-4567", retrieved.Phone);
        Assert.Equal("test@restaurant.com", retrieved.Email);
    }

    [Fact]
    public async Task CanCreateAndRetrieveTable()
    {
        // Arrange
        var restaurant = new Restaurant
        {
            Name = "Test Restaurant",
            Address = "123 Test St",
            Phone = "(555) 123-4567",
            Email = "test@restaurant.com"
        };

        var table = new Table
        {
            Number = 1,
            Capacity = 4,
            Status = TableStatus.Available,
            Restaurant = restaurant
        };

        // Act
        _context.Restaurants.Add(restaurant);
        _context.Tables.Add(table);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Tables
            .Include(t => t.Restaurant)
            .FirstAsync();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(1, retrieved.Number);
        Assert.Equal(4, retrieved.Capacity);
        Assert.Equal(TableStatus.Available, retrieved.Status);
        Assert.Equal("Test Restaurant", retrieved.Restaurant.Name);
    }

    [Fact]
    public async Task CanCreateAndRetrieveMenuItem()
    {
        // Arrange
        var restaurant = new Restaurant
        {
            Name = "Test Restaurant",
            Address = "123 Test St",
            Phone = "(555) 123-4567",
            Email = "test@restaurant.com"
        };

        var menuItem = new MenuItem
        {
            Name = "Test Pizza",
            Description = "A delicious test pizza",
            Price = 12.99m,
            Category = "Pizza",
            IsAvailable = true,
            PreparationTimeMinutes = 20,
            Restaurant = restaurant
        };

        // Act
        _context.Restaurants.Add(restaurant);
        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        var retrieved = await _context.MenuItems
            .Include(m => m.Restaurant)
            .FirstAsync();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal("Test Pizza", retrieved.Name);
        Assert.Equal("A delicious test pizza", retrieved.Description);
        Assert.Equal(12.99m, retrieved.Price);
        Assert.Equal("Pizza", retrieved.Category);
        Assert.True(retrieved.IsAvailable);
        Assert.Equal(20, retrieved.PreparationTimeMinutes);
        Assert.Equal("Test Restaurant", retrieved.Restaurant.Name);
    }

    [Fact]
    public async Task CanCreateAndRetrieveOrderWithItems()
    {
        // Arrange
        var restaurant = new Restaurant
        {
            Name = "Test Restaurant",
            Address = "123 Test St",
            Phone = "(555) 123-4567",
            Email = "test@restaurant.com"
        };

        var table = new Table
        {
            Number = 1,
            Capacity = 4,
            Status = TableStatus.Available,
            Restaurant = restaurant
        };

        var menuItem = new MenuItem
        {
            Name = "Test Pizza",
            Description = "A delicious test pizza",
            Price = 12.99m,
            Category = "Pizza",
            IsAvailable = true,
            PreparationTimeMinutes = 20,
            Restaurant = restaurant
        };

        var order = new Order
        {
            Table = table,
            Status = OrderStatus.Received,
            SubTotal = 12.99m,
            TaxAmount = 1.04m,
            TotalAmount = 14.03m,
            Remarks = "Extra cheese"
        };

        var orderItem = new OrderItem
        {
            Order = order,
            MenuItem = menuItem,
            Quantity = 1,
            UnitPrice = 12.99m,
            SpecialInstructions = "Extra cheese"
        };

        // Act
        _context.Restaurants.Add(restaurant);
        _context.Tables.Add(table);
        _context.MenuItems.Add(menuItem);
        _context.Orders.Add(order);
        _context.OrderItems.Add(orderItem);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .FirstAsync();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(1, retrieved.Table.Number);
        Assert.Equal(OrderStatus.Received, retrieved.Status);
        Assert.Equal(12.99m, retrieved.SubTotal);
        Assert.Equal(1.04m, retrieved.TaxAmount);
        Assert.Equal(14.03m, retrieved.TotalAmount);
        Assert.Equal("Extra cheese", retrieved.Remarks);
        Assert.Single(retrieved.OrderItems);
        Assert.Equal("Test Pizza", retrieved.OrderItems.First().MenuItem.Name);
        Assert.Equal(1, retrieved.OrderItems.First().Quantity);
    }

    [Fact]
    public async Task CanCreateAndRetrievePayment()
    {
        // Arrange
        var restaurant = new Restaurant
        {
            Name = "Test Restaurant",
            Address = "123 Test St",
            Phone = "(555) 123-4567",
            Email = "test@restaurant.com"
        };

        var table = new Table
        {
            Number = 1,
            Capacity = 4,
            Status = TableStatus.Available,
            Restaurant = restaurant
        };

        var order = new Order
        {
            Table = table,
            Status = OrderStatus.Received,
            SubTotal = 12.99m,
            TaxAmount = 1.04m,
            TotalAmount = 14.03m
        };

        var payment = new Payment
        {
            Order = order,
            Amount = 14.03m,
            PaymentMethod = PaymentMethod.CreditCard,
            Status = PaymentStatus.Pending,
            TransactionId = "TXN_123456789"
        };

        // Act
        _context.Restaurants.Add(restaurant);
        _context.Tables.Add(table);
        _context.Orders.Add(order);
        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        var retrieved = await _context.Payments
            .Include(p => p.Order)
            .FirstAsync();

        // Assert
        Assert.NotNull(retrieved);
        Assert.Equal(14.03m, retrieved.Amount);
        Assert.Equal(PaymentMethod.CreditCard, retrieved.PaymentMethod);
        Assert.Equal(PaymentStatus.Pending, retrieved.Status);
        Assert.Equal("TXN_123456789", retrieved.TransactionId);
        Assert.Equal(1, retrieved.Order.Table.Number);
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}
