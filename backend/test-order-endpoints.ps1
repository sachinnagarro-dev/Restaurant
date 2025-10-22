# Test script for TableOrder Backend Order API endpoints

Write-Host "Testing TableOrder Backend Order API Endpoints" -ForegroundColor Green
Write-Host "===============================================" -ForegroundColor Green

$baseUrl = "http://localhost:5000"

# Test 1: Create a new order
Write-Host "`n1. Testing POST /api/orders - Create Order" -ForegroundColor Yellow
$newOrder = @{
    tableId = 1
    items = @(
        @{
            menuItemId = 1
            quantity = 2
            specialInstructions = "Extra cheese"
        },
        @{
            menuItemId = 2
            quantity = 1
            specialInstructions = "Well done"
        }
    )
    remarks = "Please hurry, we're in a rush"
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders" -Method POST -Body $newOrder -ContentType "application/json"
    $orderData = $response.Content | ConvertFrom-Json
    $orderId = $orderData.id
    Write-Host "✅ Success! Created order with ID: $orderId"
    Write-Host "   Table: $($orderData.tableNumber)"
    Write-Host "   Status: $($orderData.status)"
    Write-Host "   Total: $($orderData.totalAmount.ToString('C'))"
    Write-Host "   Items: $($orderData.items.Count)"
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get order details
Write-Host "`n2. Testing GET /api/orders/{id} - Get Order Details" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders/$orderId" -Method GET
    $orderDetails = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Retrieved order details:"
    Write-Host "   Order ID: $($orderDetails.id)"
    Write-Host "   Table: $($orderDetails.tableNumber)"
    Write-Host "   Status: $($orderDetails.status)"
    Write-Host "   Total: $($orderDetails.totalAmount.ToString('C'))"
    Write-Host "   Remarks: $($orderDetails.remarks)"
    Write-Host "   Items:"
    foreach ($item in $orderDetails.items) {
        Write-Host "     - $($item.menuItemName) x$($item.quantity) = $($item.totalPrice.ToString('C'))"
    }
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Update order status to Preparing
Write-Host "`n3. Testing PUT /api/orders/{id}/status - Update Status to Preparing" -ForegroundColor Yellow
$statusUpdate = @{
    status = "Preparing"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders/$orderId/status" -Method PUT -Body $statusUpdate -ContentType "application/json"
    $updatedOrder = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Updated order status:"
    Write-Host "   Order ID: $($updatedOrder.id)"
    Write-Host "   New Status: $($updatedOrder.status)"
    Write-Host "   Updated At: $($updatedOrder.updatedAt)"
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Update order status to Ready
Write-Host "`n4. Testing PUT /api/orders/{id}/status - Update Status to Ready" -ForegroundColor Yellow
$statusUpdate = @{
    status = "Ready"
} | ConvertTo-Json

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders/$orderId/status" -Method PUT -Body $statusUpdate -ContentType "application/json"
    $updatedOrder = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Updated order status:"
    Write-Host "   Order ID: $($updatedOrder.id)"
    Write-Host "   New Status: $($updatedOrder.status)"
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Get all orders
Write-Host "`n5. Testing GET /api/orders - Get All Orders" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders" -Method GET
    $allOrders = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Found $($allOrders.Count) orders:"
    foreach ($order in $allOrders) {
        Write-Host "   Order $($order.id): Table $($order.tableNumber) - $($order.status) - $($order.totalAmount.ToString('C'))"
    }
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Get orders by table
Write-Host "`n6. Testing GET /api/orders/table/{tableId} - Get Orders by Table" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders/table/1" -Method GET
    $tableOrders = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Found $($tableOrders.Count) orders for table 1:"
    foreach ($order in $tableOrders) {
        Write-Host "   Order $($order.id): $($order.status) - $($order.totalAmount.ToString('C'))"
    }
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 7: Get orders by status
Write-Host "`n7. Testing GET /api/orders/status/{status} - Get Orders by Status" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders/status/Ready" -Method GET
    $readyOrders = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Found $($readyOrders.Count) orders with status 'Ready':"
    foreach ($order in $readyOrders) {
        Write-Host "   Order $($order.id): Table $($order.tableNumber) - $($order.totalAmount.ToString('C'))"
    }
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 8: Test validation - Invalid table ID
Write-Host "`n8. Testing POST /api/orders - Invalid Table ID (should fail)" -ForegroundColor Yellow
$invalidOrder = @{
    tableId = 999
    items = @(
        @{
            menuItemId = 1
            quantity = 1
        }
    )
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders" -Method POST -Body $invalidOrder -ContentType "application/json"
    Write-Host "❌ Unexpected success - validation failed!"
} catch {
    Write-Host "✅ Expected failure - validation working: $($_.Exception.Message)" -ForegroundColor Green
}

# Test 9: Test validation - Empty items
Write-Host "`n9. Testing POST /api/orders - Empty Items (should fail)" -ForegroundColor Yellow
$emptyOrder = @{
    tableId = 1
    items = @()
} | ConvertTo-Json -Depth 3

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders" -Method POST -Body $emptyOrder -ContentType "application/json"
    Write-Host "❌ Unexpected success - validation failed!"
} catch {
    Write-Host "✅ Expected failure - validation working: $($_.Exception.Message)" -ForegroundColor Green
}

# Test 10: Test invalid order ID
Write-Host "`n10. Testing GET /api/orders/{id} - Invalid Order ID (should fail)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/orders/999" -Method GET
    Write-Host "❌ Unexpected success - should have failed!"
} catch {
    Write-Host "✅ Expected failure - order not found: $($_.Exception.Message)" -ForegroundColor Green
}

Write-Host "`n===============================================" -ForegroundColor Green
Write-Host "Order API Testing Complete!" -ForegroundColor Green
Write-Host "`nSignalR Hub Events:" -ForegroundColor Cyan
Write-Host "- OrderCreated: Broadcasts when a new order is created" -ForegroundColor White
Write-Host "- OrderStatusUpdated: Broadcasts when order status changes" -ForegroundColor White
Write-Host "- Kitchen Group: Receives all order events" -ForegroundColor White
Write-Host "- Table_{number} Group: Receives events for specific table" -ForegroundColor White


