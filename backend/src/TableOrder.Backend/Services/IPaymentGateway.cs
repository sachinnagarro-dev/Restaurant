using TableOrder.Backend.DTOs;

namespace TableOrder.Backend.Services;

/// <summary>
/// Interface for payment gateway integration
/// This allows for easy switching between different payment providers
/// </summary>
public interface IPaymentGateway
{
    /// <summary>
    /// Initialize a payment transaction
    /// </summary>
    /// <param name="request">Payment gateway request details</param>
    /// <returns>Payment gateway response with transaction details</returns>
    Task<PaymentGatewayResponse> InitializePaymentAsync(PaymentGatewayRequest request);

    /// <summary>
    /// Verify payment status with the gateway
    /// </summary>
    /// <param name="transactionId">Transaction ID from the gateway</param>
    /// <returns>Payment gateway response with verification details</returns>
    Task<PaymentGatewayResponse> VerifyPaymentAsync(string transactionId);

    /// <summary>
    /// Process payment callback/webhook from gateway
    /// </summary>
    /// <param name="callbackData">Raw callback data from gateway</param>
    /// <returns>Payment gateway response with processed data</returns>
    Task<PaymentGatewayResponse> ProcessCallbackAsync(string callbackData);

    /// <summary>
    /// Generate QR code data for UPI payments
    /// </summary>
    /// <param name="amount">Payment amount</param>
    /// <param name="merchantId">Merchant ID</param>
    /// <param name="transactionNote">Transaction note</param>
    /// <returns>QR code data string</returns>
    string GenerateUpiQrCode(decimal amount, string merchantId, string transactionNote);

    /// <summary>
    /// Get payment gateway name
    /// </summary>
    string GatewayName { get; }
}
