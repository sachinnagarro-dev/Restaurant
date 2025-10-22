# Test script for Payment API endpoints
# Make sure the backend is running on http://localhost:5000

Write-Host "Testing Payment API Endpoints..." -ForegroundColor Green

# Test 1: Create an order first
Write-Host "`n1. Creating a test order..." -ForegroundColor Yellow
$orderData = @{
    tableId = 1
    items = @(
        @{
            menuItemId = 1
            quantity = 2
            specialInstructions = "Extra spicy"
        },
        @{
            menuItemId = 2
            quantity = 1
            specialInstructions = ""
        }
    )
    specialInstructions = "Test order for payment"
} | ConvertTo-Json -Depth 3

try {
    $orderResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/orders" -Method POST -Body $orderData -ContentType "application/json"
    $orderId = $orderResponse.orderId
    Write-Host "Order created with ID: $orderId" -ForegroundColor Green
} catch {
    Write-Host "Error creating order: $($_.Exception.Message)" -ForegroundColor Red
    exit 1
}

# Test 2: Generate QR code for payment
Write-Host "`n2. Generating QR code for payment..." -ForegroundColor Yellow
try {
    $qrResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/payments/qrcode/$orderId" -Method GET
    Write-Host "QR Code generated successfully:" -ForegroundColor Green
    Write-Host "  Amount: $($qrResponse.amount)" -ForegroundColor Cyan
    Write-Host "  Order ID: $($qrResponse.orderId)" -ForegroundColor Cyan
    Write-Host "  Merchant ID: $($qrResponse.merchantId)" -ForegroundColor Cyan
    Write-Host "  UPI ID: $($qrResponse.upiId)" -ForegroundColor Cyan
    Write-Host "  Transaction Note: $($qrResponse.transactionNote)" -ForegroundColor Cyan
    Write-Host "  QR Data: $($qrResponse.qrData)" -ForegroundColor Cyan
} catch {
    Write-Host "Error generating QR code: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Confirm payment
Write-Host "`n3. Confirming payment..." -ForegroundColor Yellow
$paymentConfirmation = @{
    orderId = $orderId
    transactionId = "TXN_$(Get-Date -Format 'yyyyMMddHHmmss')_$(Get-Random -Maximum 10000)"
    status = "Success"
    amount = $qrResponse.amount
    paymentMethod = "UPI"
    gatewayResponse = "Payment successful"
} | ConvertTo-Json

try {
    $paymentResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/payments/confirm" -Method POST -Body $paymentConfirmation -ContentType "application/json"
    Write-Host "Payment confirmed successfully:" -ForegroundColor Green
    Write-Host "  Payment ID: $($paymentResponse.paymentId)" -ForegroundColor Cyan
    Write-Host "  Order ID: $($paymentResponse.orderId)" -ForegroundColor Cyan
    Write-Host "  Amount: $($paymentResponse.amount)" -ForegroundColor Cyan
    Write-Host "  Payment Method: $($paymentResponse.paymentMethod)" -ForegroundColor Cyan
    Write-Host "  Status: $($paymentResponse.status)" -ForegroundColor Cyan
    Write-Host "  Transaction ID: $($paymentResponse.transactionId)" -ForegroundColor Cyan
    Write-Host "  Created At: $($paymentResponse.createdAt)" -ForegroundColor Cyan
    Write-Host "  Completed At: $($paymentResponse.completedAt)" -ForegroundColor Cyan
} catch {
    Write-Host "Error confirming payment: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Get payment status
Write-Host "`n4. Getting payment status..." -ForegroundColor Yellow
try {
    $statusResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/payments/status/$orderId" -Method GET
    Write-Host "Payment status retrieved successfully:" -ForegroundColor Green
    foreach ($payment in $statusResponse) {
        Write-Host "  Payment ID: $($payment.paymentId)" -ForegroundColor Cyan
        Write-Host "  Amount: $($payment.amount)" -ForegroundColor Cyan
        Write-Host "  Status: $($payment.status)" -ForegroundColor Cyan
        Write-Host "  Transaction ID: $($payment.transactionId)" -ForegroundColor Cyan
        Write-Host "  ---" -ForegroundColor Gray
    }
} catch {
    Write-Host "Error getting payment status: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Test payment failure scenario
Write-Host "`n5. Testing payment failure scenario..." -ForegroundColor Yellow
$failedPaymentConfirmation = @{
    orderId = $orderId
    transactionId = "TXN_FAILED_$(Get-Date -Format 'yyyyMMddHHmmss')"
    status = "Failed"
    amount = $qrResponse.amount
    paymentMethod = "UPI"
    gatewayResponse = "Payment failed - insufficient funds"
} | ConvertTo-Json

try {
    $failedPaymentResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/payments/confirm" -Method POST -Body $failedPaymentConfirmation -ContentType "application/json"
    Write-Host "Failed payment processed successfully:" -ForegroundColor Green
    Write-Host "  Payment ID: $($failedPaymentResponse.paymentId)" -ForegroundColor Cyan
    Write-Host "  Status: $($failedPaymentResponse.status)" -ForegroundColor Cyan
    Write-Host "  Transaction ID: $($failedPaymentResponse.transactionId)" -ForegroundColor Cyan
} catch {
    Write-Host "Error processing failed payment: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Get all payments (admin endpoint)
Write-Host "`n6. Getting all payments..." -ForegroundColor Yellow
try {
    $allPaymentsResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/payments" -Method GET
    Write-Host "All payments retrieved successfully:" -ForegroundColor Green
    Write-Host "  Total payments: $($allPaymentsResponse.Count)" -ForegroundColor Cyan
    foreach ($payment in $allPaymentsResponse) {
        Write-Host "  Payment ID: $($payment.paymentId), Order ID: $($payment.orderId), Amount: $($payment.amount), Status: $($payment.status)" -ForegroundColor Cyan
    }
} catch {
    Write-Host "Error getting all payments: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Test invalid order ID
Write-Host "`n7. Testing invalid order ID..." -ForegroundColor Yellow
try {
    $invalidQrResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/payments/qrcode/99999" -Method GET
    Write-Host "Unexpected: QR code generated for invalid order" -ForegroundColor Red
} catch {
    Write-Host "Expected error for invalid order ID: $($_.Exception.Message)" -ForegroundColor Green
}

# Test 8: Test duplicate transaction
Write-Host "`n8. Testing duplicate transaction..." -ForegroundColor Yellow
$duplicatePaymentConfirmation = @{
    orderId = $orderId
    transactionId = "TXN_$(Get-Date -Format 'yyyyMMddHHmmss')_$(Get-Random -Maximum 10000)"
    status = "Success"
    amount = $qrResponse.amount
    paymentMethod = "UPI"
} | ConvertTo-Json

try {
    $duplicateResponse = Invoke-RestMethod -Uri "http://localhost:5000/api/payments/confirm" -Method POST -Body $duplicatePaymentConfirmation -ContentType "application/json"
    Write-Host "Duplicate payment processed successfully:" -ForegroundColor Green
    Write-Host "  Payment ID: $($duplicateResponse.paymentId)" -ForegroundColor Cyan
} catch {
    Write-Host "Expected error for duplicate transaction: $($_.Exception.Message)" -ForegroundColor Green
}

Write-Host "`nPayment API testing completed!" -ForegroundColor Green
Write-Host "Check the SignalR hub for real-time payment status updates." -ForegroundColor Cyan
