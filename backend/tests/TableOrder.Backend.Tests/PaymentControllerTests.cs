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
using TableOrder.Backend.Services;

namespace TableOrder.Backend.Tests;

public class PaymentControllerTests : IDisposable
{
    private readonly TableOrderDbContext _context;
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PaymentController> _logger;
    private readonly PaymentController _controller;

    public PaymentControllerTests()
    {
        var options = new DbContextOptionsBuilder<TableOrderDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        _context = new TableOrderDbContext(options);
        _hubContext = new MockHubContext<OrderHub>();
        _paymentGateway = new MockPaymentGateway(new MockLogger<MockPaymentGateway>());
        _logger = new MockLogger<PaymentController>();
        _controller = new PaymentController(_context, _hubContext, _paymentGateway, _logger);

        SeedTestData();
    }

    [Fact]
    public async Task GetPaymentQrCode_ValidOrderId_ReturnsQrCodeData()
    {
        // Arrange
        var orderId = await CreateTestOrder();

        // Act
        var result = await _controller.GetPaymentQrCode(orderId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);

        var qrCodeData = (PaymentQrCodeDto)okResult.Value!;
        Assert.Equal(orderId, qrCodeData.OrderId);
        Assert.True(qrCodeData.Amount > 0);
        Assert.NotEmpty(qrCodeData.QrData);
        Assert.Contains("upi://pay", qrCodeData.QrData);
    }

    [Fact]
    public async Task GetPaymentQrCode_InvalidOrderId_ReturnsNotFound()
    {
        // Act
        var result = await _controller.GetPaymentQrCode(999);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        var notFoundResult = (NotFoundObjectResult)result.Result!;
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task GetPaymentQrCode_AlreadyPaidOrder_ReturnsBadRequest()
    {
        // Arrange
        var orderId = await CreateTestOrder();
        await CreateCompletedPayment(orderId);

        // Act
        var result = await _controller.GetPaymentQrCode(orderId);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ConfirmPayment_ValidRequest_ReturnsPaymentStatus()
    {
        // Arrange
        var orderId = await CreateTestOrder();
        var confirmationRequest = new PaymentConfirmationDto
        {
            OrderId = orderId,
            TransactionId = "TXN_SUCCESS_123456789",
            Status = "Success",
            Amount = 28.06m,
            PaymentMethod = "UPI"
        };

        // Act
        var result = await _controller.ConfirmPayment(confirmationRequest);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);

        var paymentStatus = (PaymentStatusDto)okResult.Value!;
        Assert.Equal(orderId, paymentStatus.OrderId);
        Assert.Equal("TXN_SUCCESS_123456789", paymentStatus.TransactionId);
        Assert.Equal(28.06m, paymentStatus.Amount);
        Assert.NotNull(paymentStatus.CompletedAt);
    }

    [Fact]
    public async Task ConfirmPayment_InvalidOrderId_ReturnsNotFound()
    {
        // Arrange
        var confirmationRequest = new PaymentConfirmationDto
        {
            OrderId = 999,
            TransactionId = "TXN_123456789",
            Status = "Success"
        };

        // Act
        var result = await _controller.ConfirmPayment(confirmationRequest);

        // Assert
        Assert.IsType<NotFoundObjectResult>(result.Result);
        var notFoundResult = (NotFoundObjectResult)result.Result!;
        Assert.Equal(404, notFoundResult.StatusCode);
    }

    [Fact]
    public async Task ConfirmPayment_DuplicateTransaction_ReturnsBadRequest()
    {
        // Arrange
        var orderId = await CreateTestOrder();
        var confirmationRequest = new PaymentConfirmationDto
        {
            OrderId = orderId,
            TransactionId = "TXN_DUPLICATE",
            Status = "Success"
        };

        // Create first payment
        await _controller.ConfirmPayment(confirmationRequest);

        // Act - Try to confirm same transaction again
        var result = await _controller.ConfirmPayment(confirmationRequest);

        // Assert
        Assert.IsType<BadRequestObjectResult>(result.Result);
        var badRequestResult = (BadRequestObjectResult)result.Result!;
        Assert.Equal(400, badRequestResult.StatusCode);
    }

    [Fact]
    public async Task ConfirmPayment_FailedPayment_ReturnsPaymentStatus()
    {
        // Arrange
        var orderId = await CreateTestOrder();
        var confirmationRequest = new PaymentConfirmationDto
        {
            OrderId = orderId,
            TransactionId = "TXN_FAILED_123",
            Status = "Failed",
            Amount = 28.06m
        };

        // Act
        var result = await _controller.ConfirmPayment(confirmationRequest);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);

        var paymentStatus = (PaymentStatusDto)okResult.Value!;
        Assert.Equal(orderId, paymentStatus.OrderId);
        Assert.Equal("Failed", paymentStatus.Status);
        Assert.Null(paymentStatus.CompletedAt);
    }

    [Fact]
    public async Task GetPaymentStatus_ValidOrderId_ReturnsPayments()
    {
        // Arrange
        var orderId = await CreateTestOrder();
        await CreateCompletedPayment(orderId);

        // Act
        var result = await _controller.GetPaymentStatus(orderId);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);

        var payments = (IEnumerable<PaymentStatusDto>)okResult.Value!;
        Assert.Single(payments);
        Assert.Equal(orderId, payments.First().OrderId);
    }

    [Fact]
    public async Task GetAllPayments_ReturnsAllPayments()
    {
        // Arrange
        var orderId1 = await CreateTestOrder();
        var orderId2 = await CreateTestOrder();
        await CreateCompletedPayment(orderId1);
        await CreateCompletedPayment(orderId2);

        // Act
        var result = await _controller.GetAllPayments();

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);
        var okResult = (OkObjectResult)result.Result!;
        Assert.Equal(200, okResult.StatusCode);

        var payments = (IEnumerable<PaymentStatusDto>)okResult.Value!;
        Assert.Equal(2, payments.Count());
    }

    [Fact]
    public async Task ConfirmPayment_UpdatesOrderStatus_WhenPaymentSuccessful()
    {
        // Arrange
        var orderId = await CreateTestOrder();
        var confirmationRequest = new PaymentConfirmationDto
        {
            OrderId = orderId,
            TransactionId = "TXN_SUCCESS_STATUS_123",
            Status = "Success",
            Amount = 28.06m
        };

        // Act
        var result = await _controller.ConfirmPayment(confirmationRequest);

        // Assert
        Assert.IsType<OkObjectResult>(result.Result);

        // Verify order status was updated
        var order = await _context.Orders.FindAsync(orderId);
        Assert.NotNull(order);
        Assert.Equal(OrderStatus.Closed, order.Status);
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

    private async Task CreateCompletedPayment(int orderId)
    {
        var payment = new Payment
        {
            OrderId = orderId,
            Amount = 28.06m,
            PaymentMethod = PaymentMethod.CreditCard,
            Status = PaymentStatus.Completed,
            TransactionId = "TXN_COMPLETED_123",
            ReferenceNumber = "REF_123",
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();
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
