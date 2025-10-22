using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.Services;

public interface IPaymentService
{
    Task<PaymentResponse?> GetPaymentByIdAsync(int id);
    Task<IEnumerable<PaymentResponse>> GetPaymentsByOrderAsync(int orderId);
    Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request);
    Task<PaymentResponse?> UpdatePaymentStatusAsync(int id, PaymentStatus status);
}

public class PaymentService : IPaymentService
{
    private readonly TableOrderDbContext _context;
    private readonly IHubContext<Hubs.OrderHub> _hubContext;

    public PaymentService(TableOrderDbContext context, IHubContext<Hubs.OrderHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<PaymentResponse?> GetPaymentByIdAsync(int id)
    {
        var payment = await _context.Payments
            .Include(p => p.Order)
            .FirstOrDefaultAsync(p => p.Id == id);

        return payment == null ? null : MapToPaymentResponse(payment);
    }

    public async Task<IEnumerable<PaymentResponse>> GetPaymentsByOrderAsync(int orderId)
    {
        var payments = await _context.Payments
            .Where(p => p.OrderId == orderId)
            .OrderByDescending(p => p.CreatedAt)
            .ToListAsync();

        return payments.Select(MapToPaymentResponse);
    }

    public async Task<PaymentResponse> CreatePaymentAsync(PaymentRequest request)
    {
        var order = await _context.Orders.FindAsync(request.OrderId);
        if (order == null)
            throw new ArgumentException("Order not found");

        if (!Enum.TryParse<PaymentMethod>(request.PaymentMethod, out var paymentMethod))
            throw new ArgumentException("Invalid payment method");

        var payment = new Payment
        {
            OrderId = request.OrderId,
            Amount = request.Amount,
            PaymentMethod = paymentMethod,
            Status = PaymentStatus.Pending,
            TransactionId = GenerateTransactionId(),
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        // Simulate payment processing
        await ProcessPaymentAsync(payment);

        return MapToPaymentResponse(payment);
    }

    public async Task<PaymentResponse?> UpdatePaymentStatusAsync(int id, PaymentStatus status)
    {
        var payment = await _context.Payments.FindAsync(id);
        if (payment == null)
            return null;

        payment.Status = status;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Send real-time update
        await _hubContext.Clients.Group("Kitchen").SendAsync("PaymentStatusUpdated", id, status.ToString());
        await _hubContext.Clients.Group($"Table_{payment.Order.Table.Number}").SendAsync("PaymentStatusUpdated", id, status.ToString());

        return MapToPaymentResponse(payment);
    }

    private async Task ProcessPaymentAsync(Payment payment)
    {
        // Simulate payment processing delay
        await Task.Delay(2000);

        // Simulate success (in real implementation, integrate with payment gateway)
        payment.Status = PaymentStatus.Completed;
        payment.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
    }

    private static string GenerateTransactionId()
    {
        return $"TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
    }

    private static PaymentResponse MapToPaymentResponse(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            PaymentMethod = payment.PaymentMethod.ToString(),
            Status = payment.Status.ToString(),
            TransactionId = payment.TransactionId,
            ReferenceNumber = payment.ReferenceNumber,
            CreatedAt = payment.CreatedAt
        };
    }
}
