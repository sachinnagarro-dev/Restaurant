using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Hubs;
using TableOrder.Backend.Models;
using TableOrder.Backend.Services;

namespace TableOrder.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class PaymentController : ControllerBase
{
    private readonly TableOrderDbContext _context;
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly IPaymentGateway _paymentGateway;
    private readonly ILogger<PaymentController> _logger;

    public PaymentController(
        TableOrderDbContext context,
        IHubContext<OrderHub> hubContext,
        IPaymentGateway paymentGateway,
        ILogger<PaymentController> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _paymentGateway = paymentGateway;
        _logger = logger;
    }

    /// <summary>
    /// Generate QR code for UPI payment
    /// </summary>
    [HttpGet("qrcode/{orderId}")]
    public async Task<ActionResult<PaymentQrCodeDto>> GetPaymentQrCode(int orderId)
    {
        try
        {
            // Get order details
            var order = await _context.Orders
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == orderId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            // Check if order is already paid
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == orderId && p.Status == PaymentStatus.Completed);

            if (existingPayment != null)
            {
                return BadRequest(new { message = "Order is already paid" });
            }

            // Generate QR code data
            var qrCodeData = _paymentGateway.GenerateUpiQrCode(
                order.TotalAmount,
                "MOCK_MERCHANT_123",
                $"Order #{order.Id} - Table {order.Table.Number}"
            );

            var response = new PaymentQrCodeDto
            {
                QrData = qrCodeData,
                Amount = order.TotalAmount,
                OrderId = order.Id,
                MerchantId = "MOCK_MERCHANT_123",
                UpiId = "tableorder@mockupi",
                TransactionNote = $"Order #{order.Id} - Table {order.Table.Number}"
            };

            _logger.LogInformation($"Generated QR code for order {orderId}, amount: {order.TotalAmount:C}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error generating QR code for order {orderId}");
            return StatusCode(500, new { message = "An error occurred while generating the QR code" });
        }
    }

    /// <summary>
    /// Confirm payment and update order status
    /// </summary>
    [HttpPost("confirm")]
    public async Task<ActionResult<PaymentStatusDto>> ConfirmPayment([FromBody] PaymentConfirmationDto request)
    {
        try
        {
            // Get order details
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.Payments)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            // Check if payment already exists
            var existingPayment = await _context.Payments
                .FirstOrDefaultAsync(p => p.OrderId == request.OrderId && p.TransactionId == request.TransactionId);

            if (existingPayment != null)
            {
                return BadRequest(new { message = "Payment already processed" });
            }

            // Verify payment with gateway (mock verification for prototype)
            var verificationResult = await _paymentGateway.VerifyPaymentAsync(request.TransactionId);

            if (!verificationResult.Success)
            {
                return BadRequest(new { message = "Payment verification failed" });
            }

            // Determine payment status
            var paymentStatus = request.Status.ToLower() switch
            {
                "success" or "completed" or "paid" => PaymentStatus.Completed,
                "failed" or "cancelled" => PaymentStatus.Failed,
                "pending" => PaymentStatus.Pending,
                _ => PaymentStatus.Pending
            };

            // Create payment record
            var payment = new Payment
            {
                OrderId = request.OrderId,
                Amount = request.Amount ?? order.TotalAmount,
                PaymentMethod = ParsePaymentMethod(request.PaymentMethod),
                Status = paymentStatus,
                TransactionId = request.TransactionId,
                ReferenceNumber = GenerateReferenceNumber(),
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Payments.Add(payment);

            // Update order status if payment is successful
            if (paymentStatus == PaymentStatus.Completed)
            {
                order.Status = OrderStatus.Closed;
                order.UpdatedAt = DateTime.UtcNow;

                // Broadcast payment success
                await BroadcastPaymentEvent("PaymentCompleted", new
                {
                    OrderId = order.Id,
                    TableId = order.TableId,
                    TableNumber = order.Table.Number,
                    PaymentId = payment.Id,
                    Amount = payment.Amount,
                    TransactionId = payment.TransactionId,
                    CompletedAt = DateTime.UtcNow
                });
            }
            else
            {
                // Broadcast payment failure
                await BroadcastPaymentEvent("PaymentFailed", new
                {
                    OrderId = order.Id,
                    TableId = order.TableId,
                    TableNumber = order.Table.Number,
                    PaymentId = payment.Id,
                    Amount = payment.Amount,
                    TransactionId = payment.TransactionId,
                    Reason = verificationResult.ErrorMessage
                });
            }

            await _context.SaveChangesAsync();

            var response = new PaymentStatusDto
            {
                PaymentId = payment.Id,
                OrderId = payment.OrderId,
                Amount = payment.Amount,
                PaymentMethod = payment.PaymentMethod.ToString(),
                Status = payment.Status.ToString(),
                TransactionId = payment.TransactionId,
                CreatedAt = payment.CreatedAt,
                CompletedAt = paymentStatus == PaymentStatus.Completed ? DateTime.UtcNow : null
            };

            _logger.LogInformation($"Payment confirmed for order {request.OrderId}, status: {paymentStatus}, transaction: {request.TransactionId}");

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error confirming payment for order {request.OrderId}");
            return StatusCode(500, new { message = "An error occurred while confirming the payment" });
        }
    }

    /// <summary>
    /// Get payment status for an order
    /// </summary>
    [HttpGet("status/{orderId}")]
    public async Task<ActionResult<IEnumerable<PaymentStatusDto>>> GetPaymentStatus(int orderId)
    {
        try
        {
            var payments = await _context.Payments
                .Where(p => p.OrderId == orderId)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var response = payments.Select(p => new PaymentStatusDto
            {
                PaymentId = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                TransactionId = p.TransactionId,
                CreatedAt = p.CreatedAt,
                CompletedAt = p.Status == PaymentStatus.Completed ? p.UpdatedAt : null
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving payment status for order {orderId}");
            return StatusCode(500, new { message = "An error occurred while retrieving payment status" });
        }
    }

    /// <summary>
    /// Get all payments (admin endpoint)
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<PaymentStatusDto>>> GetAllPayments()
    {
        try
        {
            var payments = await _context.Payments
                .Include(p => p.Order)
                .OrderByDescending(p => p.CreatedAt)
                .ToListAsync();

            var response = payments.Select(p => new PaymentStatusDto
            {
                PaymentId = p.Id,
                OrderId = p.OrderId,
                Amount = p.Amount,
                PaymentMethod = p.PaymentMethod.ToString(),
                Status = p.Status.ToString(),
                TransactionId = p.TransactionId,
                CreatedAt = p.CreatedAt,
                CompletedAt = p.Status == PaymentStatus.Completed ? p.UpdatedAt : null
            });

            return Ok(response);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all payments");
            return StatusCode(500, new { message = "An error occurred while retrieving payments" });
        }
    }

    private async Task BroadcastPaymentEvent(string eventName, object data)
    {
        try
        {
            // Broadcast to kitchen group
            await _hubContext.Clients.Group("Kitchen").SendAsync(eventName, data);

            // Broadcast to specific table group
            if (data is not null)
            {
                var dataType = data.GetType();
                var tableNumberProperty = dataType.GetProperty("TableNumber");
                if (tableNumberProperty != null)
                {
                    var tableNumber = tableNumberProperty.GetValue(data);
                    if (tableNumber != null)
                    {
                        await _hubContext.Clients.Group($"Table_{tableNumber}").SendAsync(eventName, data);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting payment event: {eventName}");
        }
    }

    private static PaymentMethod ParsePaymentMethod(string? method)
    {
        if (string.IsNullOrEmpty(method))
            return PaymentMethod.CreditCard;

        return method.ToLower() switch
        {
            "upi" or "upi_pay" => PaymentMethod.Office365, // Using Office365 as UPI placeholder
            "credit_card" or "card" => PaymentMethod.CreditCard,
            "debit_card" => PaymentMethod.DebitCard,
            "cash" => PaymentMethod.Cash,
            "apple_pay" => PaymentMethod.ApplePay,
            "google_pay" => PaymentMethod.GooglePay,
            _ => PaymentMethod.CreditCard
        };
    }

    private static string GenerateReferenceNumber()
    {
        return $"REF_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
    }
}
