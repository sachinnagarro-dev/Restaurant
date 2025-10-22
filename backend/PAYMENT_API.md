# Payment API Documentation

This document describes the Payment API endpoints for the TableOrder system.

## Overview

The Payment API provides endpoints for:
- Generating UPI QR codes for payments
- Confirming payment transactions
- Retrieving payment status
- Managing payment records

## Base URL

```
http://localhost:5000/api/payments
```

## Endpoints

### 1. Generate Payment QR Code

**GET** `/api/payments/qrcode/{orderId}`

Generates a UPI QR code for payment of a specific order.

#### Parameters

- `orderId` (path): The ID of the order to pay for

#### Response

```json
{
  "qrData": "upi://pay?pa=tableorder@mockupi&pn=TableOrder Restaurant&am=28.06&cu=INR&tn=Order #123 - Table 1",
  "amount": 28.06,
  "orderId": 123,
  "merchantId": "MOCK_MERCHANT_123",
  "upiId": "tableorder@mockupi",
  "transactionNote": "Order #123 - Table 1"
}
```

#### Status Codes

- `200 OK`: QR code generated successfully
- `404 Not Found`: Order not found
- `400 Bad Request`: Order is already paid

#### Example

```bash
curl -X GET "http://localhost:5000/api/payments/qrcode/123"
```

### 2. Confirm Payment

**POST** `/api/payments/confirm`

Confirms a payment transaction and updates the order status.

#### Request Body

```json
{
  "orderId": 123,
  "transactionId": "TXN_123456789",
  "status": "Success",
  "amount": 28.06,
  "paymentMethod": "UPI",
  "gatewayResponse": "Payment successful"
}
```

#### Parameters

- `orderId` (required): The ID of the order being paid
- `transactionId` (required): Transaction ID from payment gateway
- `status` (required): Payment status ("Success", "Failed", "Pending")
- `amount` (optional): Payment amount (defaults to order total)
- `paymentMethod` (optional): Payment method used
- `gatewayResponse` (optional): Response from payment gateway

#### Response

```json
{
  "paymentId": 456,
  "orderId": 123,
  "amount": 28.06,
  "paymentMethod": "UPI",
  "status": "Completed",
  "transactionId": "TXN_123456789",
  "createdAt": "2024-01-15T10:30:00Z",
  "completedAt": "2024-01-15T10:30:05Z"
}
```

#### Status Codes

- `200 OK`: Payment confirmed successfully
- `404 Not Found`: Order not found
- `400 Bad Request`: Payment already processed or verification failed

#### Example

```bash
curl -X POST "http://localhost:5000/api/payments/confirm" \
  -H "Content-Type: application/json" \
  -d '{
    "orderId": 123,
    "transactionId": "TXN_123456789",
    "status": "Success",
    "amount": 28.06,
    "paymentMethod": "UPI"
  }'
```

### 3. Get Payment Status

**GET** `/api/payments/status/{orderId}`

Retrieves payment status for a specific order.

#### Parameters

- `orderId` (path): The ID of the order

#### Response

```json
[
  {
    "paymentId": 456,
    "orderId": 123,
    "amount": 28.06,
    "paymentMethod": "UPI",
    "status": "Completed",
    "transactionId": "TXN_123456789",
    "createdAt": "2024-01-15T10:30:00Z",
    "completedAt": "2024-01-15T10:30:05Z"
  }
]
```

#### Status Codes

- `200 OK`: Payment status retrieved successfully
- `500 Internal Server Error`: Error retrieving payment status

#### Example

```bash
curl -X GET "http://localhost:5000/api/payments/status/123"
```

### 4. Get All Payments

**GET** `/api/payments`

Retrieves all payments (admin endpoint).

#### Response

```json
[
  {
    "paymentId": 456,
    "orderId": 123,
    "amount": 28.06,
    "paymentMethod": "UPI",
    "status": "Completed",
    "transactionId": "TXN_123456789",
    "createdAt": "2024-01-15T10:30:00Z",
    "completedAt": "2024-01-15T10:30:05Z"
  }
]
```

#### Status Codes

- `200 OK`: Payments retrieved successfully
- `500 Internal Server Error`: Error retrieving payments

#### Example

```bash
curl -X GET "http://localhost:5000/api/payments"
```

## Payment Status Values

- `Pending`: Payment initiated but not completed
- `Completed`: Payment successful
- `Failed`: Payment failed
- `Cancelled`: Payment cancelled

## Payment Methods

- `CreditCard`: Credit card payment
- `DebitCard`: Debit card payment
- `UPI`: UPI payment
- `Cash`: Cash payment
- `ApplePay`: Apple Pay
- `GooglePay`: Google Pay

## Real-time Updates

The Payment API integrates with SignalR for real-time updates:

### Events Broadcasted

1. **PaymentCompleted**: When a payment is successfully completed
   ```json
   {
     "orderId": 123,
     "tableId": 1,
     "tableNumber": 1,
     "paymentId": 456,
     "amount": 28.06,
     "transactionId": "TXN_123456789",
     "completedAt": "2024-01-15T10:30:05Z"
   }
   ```

2. **PaymentFailed**: When a payment fails
   ```json
   {
     "orderId": 123,
     "tableId": 1,
     "tableNumber": 1,
     "paymentId": 456,
     "amount": 28.06,
     "transactionId": "TXN_123456789",
     "reason": "Payment failed due to insufficient funds"
   }
   ```

### SignalR Groups

- `Kitchen`: Kitchen staff receive payment updates
- `Table_{tableNumber}`: Table-specific payment updates

## Error Handling

The API returns appropriate HTTP status codes and error messages:

```json
{
  "message": "Order not found"
}
```

Common error scenarios:
- Order not found (404)
- Order already paid (400)
- Payment verification failed (400)
- Invalid transaction ID (400)
- Server errors (500)

## Testing

Use the provided test script to test the payment endpoints:

```powershell
.\test-payment-endpoints.ps1
```

## Future Enhancements

The payment system is designed to easily integrate with real payment gateways:

1. **Razorpay Integration**: Replace `MockPaymentGateway` with `RazorpayPaymentGateway`
2. **Cashfree Integration**: Replace `MockPaymentGateway` with `CashfreePaymentGateway`
3. **Multiple Payment Methods**: Support for various payment methods
4. **Payment Refunds**: Add refund functionality
5. **Payment Analytics**: Add payment reporting and analytics

## Security Considerations

- Payment gateway integration should use HTTPS
- Transaction IDs should be validated
- Payment amounts should be verified
- Admin endpoints should be protected
- Payment data should be encrypted in transit and at rest
