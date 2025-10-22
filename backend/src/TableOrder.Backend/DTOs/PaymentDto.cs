using System.ComponentModel.DataAnnotations;

namespace TableOrder.Backend.DTOs;

public class PaymentQrCodeDto
{
    public string QrData { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public int OrderId { get; set; }
    public string MerchantId { get; set; } = string.Empty;
    public string UpiId { get; set; } = string.Empty;
    public string TransactionNote { get; set; } = string.Empty;
}

public class PaymentConfirmationDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public string TransactionId { get; set; } = string.Empty;

    [Required]
    public string Status { get; set; } = string.Empty;

    public decimal? Amount { get; set; }
    public string? PaymentMethod { get; set; }
    public string? GatewayResponse { get; set; }
}

public class PaymentStatusDto
{
    public int PaymentId { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string PaymentMethod { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string TransactionId { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
    public DateTime? CompletedAt { get; set; }
}

public class PaymentGatewayRequest
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "INR";
    public string Description { get; set; } = string.Empty;
    public string CustomerId { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string CustomerPhone { get; set; } = string.Empty;
}

public class PaymentGatewayResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = string.Empty;
    public string PaymentUrl { get; set; } = string.Empty;
    public string QrCodeData { get; set; } = string.Empty;
    public string ErrorMessage { get; set; } = string.Empty;
    public Dictionary<string, object> GatewayData { get; set; } = new();
}
