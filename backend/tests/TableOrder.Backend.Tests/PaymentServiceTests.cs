using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Xunit;
using TableOrder.Backend.Data;
using TableOrder.Backend.Models;
using TableOrder.Backend.Services;

namespace TableOrder.Backend.Tests;

public class PaymentServiceTests : IDisposable
{
    private readonly TableOrderDbContext _context;
    private readonly ILogger<PaymentService> _logger;
    private readonly PaymentService _paymentService;

    public PaymentServiceTests()
    {
        var options = new DbContextOptionsBuilder<TableOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TableOrderDbContext(options);
        _logger = new MockLogger<PaymentService>();
        _paymentService = new PaymentService(_context, null!);

        SeedTestData();
    }

    [Fact]
    public async Task CreatePayment_ValidOrder_ReturnsPayment()
    {
        // Arrange
        var orderId = await CreateTestOrder();
        var paymentRequest = new DTOs.PaymentRequest
        {
            OrderId = orderId,
            Amount = 28.06m,
            PaymentMethod = "CreditCard"
        };

        // Act
        var result = await _paymentService.CreatePaymentAsync(paymentRequest);

        // Assert
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(28.06m, result.Amount);
        Assert.Equal("CreditCard", result.PaymentMethod);
        Assert.Equal("Completed", result.Status);
    }

    [Fact]
    public async Task CreatePayment_InvalidOrderId_ThrowsException()
    {
        // Arrange
        var paymentRequest = new DTOs.PaymentRequest
        {
            OrderId = 999,
            Amount = 28.06m,
            PaymentMethod = "CreditCard"
        };

        // Act & Assert
        await Assert.ThrowsAsync<ArgumentException>(() => _paymentService.CreatePaymentAsync(paymentRequest));
    }

    [Fact]
    public async Task CreatePayment_AmountMismatch_StillCreatesPayment()
    {
        // Arrange
        var orderId = await CreateTestOrder();
        var paymentRequest = new DTOs.PaymentRequest
        {
            OrderId = orderId,
            Amount = 50.00m, // Different amount
            PaymentMethod = "CreditCard"
        };

        // Act
        var result = await _paymentService.CreatePaymentAsync(paymentRequest);

        // Assert - PaymentService doesn't validate amount mismatch
        Assert.NotNull(result);
        Assert.Equal(orderId, result.OrderId);
        Assert.Equal(50.00m, result.Amount);
    }

    private async Task<int> CreateTestOrder()
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

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();
        return order.Id;
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

        _context.Restaurants.Add(restaurant);
        _context.Tables.Add(table);
        _context.SaveChanges();
    }

    public void Dispose()
    {
        _context.Dispose();
    }
}

// Mock implementations for testing
