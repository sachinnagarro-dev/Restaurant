using TableOrder.Backend.DTOs;

namespace TableOrder.Backend.Services;

/// <summary>
/// Mock payment gateway for prototype/testing purposes
/// This simulates payment gateway behavior without actual integration
/// </summary>
public class MockPaymentGateway : IPaymentGateway
{
    private readonly ILogger<MockPaymentGateway> _logger;
    private static readonly Dictionary<string, PaymentGatewayResponse> _mockTransactions = new();

    public string GatewayName => "MockPaymentGateway";

    public MockPaymentGateway(ILogger<MockPaymentGateway> logger)
    {
        _logger = logger;
    }

    public async Task<PaymentGatewayResponse> InitializePaymentAsync(PaymentGatewayRequest request)
    {
        _logger.LogInformation($"Initializing mock payment for order {request.OrderId}, amount: {request.Amount:C}");

        // Simulate async operation
        await Task.Delay(100);

        var transactionId = $"MOCK_TXN_{DateTime.UtcNow:yyyyMMddHHmmss}_{Guid.NewGuid().ToString("N")[..8]}";
        
        var response = new PaymentGatewayResponse
        {
            Success = true,
            TransactionId = transactionId,
            PaymentUrl = $"https://mock-payment-gateway.com/pay/{transactionId}",
            QrCodeData = GenerateUpiQrCode(request.Amount, "MOCK_MERCHANT_123", $"Order #{request.OrderId}"),
            GatewayData = new Dictionary<string, object>
            {
                ["merchant_id"] = "MOCK_MERCHANT_123",
                ["order_id"] = request.OrderId,
                ["amount"] = request.Amount,
                ["currency"] = request.Currency,
                ["status"] = "initiated"
            }
        };

        // Store mock transaction for verification
        _mockTransactions[transactionId] = new PaymentGatewayResponse
        {
            Success = true,
            TransactionId = transactionId,
            PaymentUrl = response.PaymentUrl,
            QrCodeData = response.QrCodeData,
            GatewayData = new Dictionary<string, object>(response.GatewayData)
        };

        _logger.LogInformation($"Mock payment initialized with transaction ID: {transactionId}");
        return response;
    }

    public async Task<PaymentGatewayResponse> VerifyPaymentAsync(string transactionId)
    {
        _logger.LogInformation($"Verifying mock payment: {transactionId}");

        await Task.Delay(50);

        if (!_mockTransactions.TryGetValue(transactionId, out var transaction))
        {
            return new PaymentGatewayResponse
            {
                Success = false,
                ErrorMessage = "Transaction not found"
            };
        }

        // For testing, always return success unless transaction ID contains "FAILED"
        var isSuccessful = !transactionId.Contains("FAILED");

        transaction.Success = isSuccessful;
        if (isSuccessful)
        {
            transaction.GatewayData["status"] = "completed";
            transaction.GatewayData["payment_id"] = $"PAY_{Guid.NewGuid().ToString("N")[..12]}";
        }
        else
        {
            transaction.GatewayData["status"] = "failed";
            transaction.ErrorMessage = "Payment failed due to insufficient funds";
        }

        _logger.LogInformation($"Mock payment verification result: {(isSuccessful ? "Success" : "Failed")}");
        return transaction;
    }

    public async Task<PaymentGatewayResponse> ProcessCallbackAsync(string callbackData)
    {
        _logger.LogInformation("Processing mock payment callback");

        await Task.Delay(50);

        // Mock callback processing - in real implementation, this would parse gateway-specific data
        try
        {
            var callbackObj = System.Text.Json.JsonSerializer.Deserialize<Dictionary<string, object>>(callbackData);
            
            if (callbackObj != null && callbackObj.ContainsKey("transaction_id"))
            {
                var transactionId = callbackObj["transaction_id"].ToString()!;
                return await VerifyPaymentAsync(transactionId);
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error processing mock payment callback");
        }

        return new PaymentGatewayResponse
        {
            Success = false,
            ErrorMessage = "Invalid callback data"
        };
    }

    public string GenerateUpiQrCode(decimal amount, string merchantId, string transactionNote)
    {
        // Generate UPI QR code data in standard format
        // Format: upi://pay?pa=merchant@upi&pn=MerchantName&am=amount&cu=INR&tn=transactionNote
        var upiId = "tableorder@mockupi"; // Mock UPI ID
        var merchantName = "TableOrder Restaurant";
        var amountStr = amount.ToString("F2");
        
        var qrData = $"upi://pay?pa={upiId}&pn={Uri.EscapeDataString(merchantName)}&am={amountStr}&cu=INR&tn={Uri.EscapeDataString(transactionNote)}";
        
        _logger.LogInformation($"Generated UPI QR code for amount: {amount:C}");
        return qrData;
    }
}
