# Order API Documentation

## Overview

The Order API provides comprehensive endpoints for managing restaurant orders with real-time updates via SignalR. The API handles order creation, status updates, and broadcasting events to kitchen displays and table tablets.

## Endpoints

### POST /api/orders
Creates a new order with items and remarks.

**Request Body:**
```json
{
  "tableId": 1,
  "items": [
    {
      "menuItemId": 1,
      "quantity": 2,
      "specialInstructions": "Extra cheese"
    },
    {
      "menuItemId": 2,
      "quantity": 1,
      "specialInstructions": "Well done"
    }
  ],
  "remarks": "Please hurry, we're in a rush"
}
```

**Validation:**
- `tableId`: Required, must be a positive number
- `items`: Required, must contain at least one item
- `items[].menuItemId`: Required, must be a positive number
- `items[].quantity`: Required, must be between 1 and 10
- `remarks`: Optional, max 500 characters

**Response (201 Created):**
```json
{
  "id": 1,
  "tableId": 1,
  "tableNumber": 1,
  "status": "Received",
  "subTotal": 25.98,
  "taxAmount": 2.08,
  "totalAmount": 28.06,
  "remarks": "Please hurry, we're in a rush",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "items": [
    {
      "id": 1,
      "menuItemId": 1,
      "menuItemName": "Margherita Pizza",
      "quantity": 2,
      "unitPrice": 12.99,
      "totalPrice": 25.98,
      "specialInstructions": "Extra cheese"
    }
  ]
}
```

### GET /api/orders/{id}
Retrieves detailed information about a specific order.

**Response (200 OK):**
```json
{
  "id": 1,
  "tableId": 1,
  "tableNumber": 1,
  "status": "Received",
  "subTotal": 25.98,
  "taxAmount": 2.08,
  "totalAmount": 28.06,
  "remarks": "Please hurry, we're in a rush",
  "createdAt": "2024-01-15T10:30:00Z",
  "updatedAt": "2024-01-15T10:30:00Z",
  "items": [...]
}
```

### PUT /api/orders/{id}/status
Updates the status of an existing order.

**Request Body:**
```json
{
  "status": "Preparing"
}
```

**Valid Status Values:**
- `Received` - Order received and confirmed
- `Preparing` - Order being prepared in kitchen
- `Ready` - Order ready for pickup
- `Served` - Order delivered to table
- `Closed` - Order completed and closed

**Response (200 OK):**
Returns the updated order with new status and timestamp.

### GET /api/orders
Retrieves all orders with summary information.

**Response (200 OK):**
```json
[
  {
    "id": 1,
    "tableId": 1,
    "tableNumber": 1,
    "status": "Received",
    "totalAmount": 28.06,
    "itemCount": 3,
    "createdAt": "2024-01-15T10:30:00Z"
  }
]
```

### GET /api/orders/table/{tableId}
Retrieves all orders for a specific table.

### GET /api/orders/status/{status}
Retrieves all orders with a specific status.

## SignalR Hub Events

The Order API broadcasts real-time events via the SignalR hub at `/orderHub`.

### Connection Setup
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/orderHub")
    .build();

await connection.start();
```

### Join Groups
```javascript
// Join kitchen group to receive all order events
await connection.invoke("JoinKitchenGroup");

// Join specific table group
await connection.invoke("JoinTableGroup", tableNumber);
```

### Event Handlers

#### OrderCreated
Triggered when a new order is created.

```javascript
connection.on("OrderCreated", (data) => {
    console.log("New order created:", data);
    // data contains: orderId, tableId, tableNumber, status, totalAmount, itemCount, createdAt
});
```

#### OrderStatusUpdated
Triggered when an order status is updated.

```javascript
connection.on("OrderStatusUpdated", (data) => {
    console.log("Order status updated:", data);
    // data contains: orderId, tableId, tableNumber, oldStatus, newStatus, updatedAt
});
```

## Error Handling

The API returns appropriate HTTP status codes and error messages:

### 400 Bad Request
```json
{
  "message": "Table not found"
}
```

```json
{
  "message": "The following items are not available: Pepperoni Pizza, BBQ Chicken"
}
```

### 404 Not Found
```json
{
  "message": "Order not found"
}
```

### 500 Internal Server Error
```json
{
  "message": "An error occurred while creating the order"
}
```

## Business Logic

### Order Creation Process
1. Validate table exists and is available
2. Validate all menu items exist and are available
3. Calculate order totals (subtotal + 8% tax)
4. Create order and order items
5. Save to database
6. Broadcast `OrderCreated` event to SignalR clients
7. Return created order details

### Status Update Process
1. Validate order exists
2. Update order status and timestamp
3. Save changes to database
4. Broadcast `OrderStatusUpdated` event to SignalR clients
5. Return updated order details

### Tax Calculation
- Tax rate: 8%
- Formula: `totalAmount = subTotal + (subTotal * 0.08)`

## Testing

Use the provided PowerShell test script:

```powershell
.\test-order-endpoints.ps1
```

This script tests all endpoints and demonstrates the complete order workflow.

## Example Usage

### Frontend Integration (JavaScript)

```javascript
// Create a new order
const createOrder = async (tableId, items, remarks) => {
    const response = await fetch('/api/orders', {
        method: 'POST',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({
            tableId,
            items,
            remarks
        })
    });
    
    if (response.ok) {
        return await response.json();
    } else {
        throw new Error('Failed to create order');
    }
};

// Update order status
const updateOrderStatus = async (orderId, status) => {
    const response = await fetch(`/api/orders/${orderId}/status`, {
        method: 'PUT',
        headers: {
            'Content-Type': 'application/json'
        },
        body: JSON.stringify({ status })
    });
    
    return await response.json();
};

// Listen for real-time updates
connection.on("OrderCreated", (data) => {
    // Update kitchen display with new order
    updateKitchenDisplay(data);
});

connection.on("OrderStatusUpdated", (data) => {
    // Update both kitchen display and table tablet
    updateKitchenDisplay(data);
    updateTableDisplay(data);
});
```

## Database Schema

### Order Table
- `Id` (Primary Key)
- `TableId` (Foreign Key to Tables)
- `Status` (Enum: Received, Preparing, Ready, Served, Closed)
- `SubTotal` (Decimal)
- `TaxAmount` (Decimal)
- `TotalAmount` (Decimal)
- `Remarks` (String, max 1000 chars)
- `CreatedAt` (DateTime)
- `UpdatedAt` (DateTime)

### OrderItem Table
- `Id` (Primary Key)
- `OrderId` (Foreign Key to Orders)
- `MenuItemId` (Foreign Key to MenuItems)
- `Quantity` (Integer)
- `UnitPrice` (Decimal)
- `SpecialInstructions` (String, max 500 chars)
- `CreatedAt` (DateTime)

## Performance Considerations

- Orders are indexed by `TableId` and `Status` for efficient querying
- SignalR events are broadcast asynchronously to prevent blocking
- Database operations use Entity Framework Core with optimized queries
- Validation is performed before database operations to minimize errors

## Security

- Input validation prevents SQL injection and malicious data
- Order creation validates table and menu item availability
- Status updates are logged for audit purposes
- SignalR connections are managed securely with proper group membership
