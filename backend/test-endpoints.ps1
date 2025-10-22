# Test script for TableOrder Backend API endpoints

Write-Host "Testing TableOrder Backend API Endpoints" -ForegroundColor Green
Write-Host "=========================================" -ForegroundColor Green

$baseUrl = "http://localhost:5000"

# Test 1: Get all menu items
Write-Host "`n1. Testing GET /api/menu" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/menu" -Method GET
    $menuData = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Found $($menuData.totalItems) menu items in $($menuData.categories.Count) categories"
    Write-Host "Categories: $($menuData.categories.name -join ', ')"
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 2: Get menu categories
Write-Host "`n2. Testing GET /api/menu/categories" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/menu/categories" -Method GET
    $categories = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Found categories: $($categories -join ', ')"
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 3: Get menu by category
Write-Host "`n3. Testing GET /api/menu/category/Pizza" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/menu/category/Pizza" -Method GET
    $pizzaItems = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Found $($pizzaItems.Count) pizza items"
    foreach ($item in $pizzaItems) {
        $vegStatus = if ($item.isVegetarian) { "🌱 Veg" } else { "🍖 Non-Veg" }
        Write-Host "   - $($item.name) - $($item.price.ToString('C')) - $vegStatus"
    }
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 4: Get specific menu item
Write-Host "`n4. Testing GET /api/menu/1" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/menu/1" -Method GET
    $menuItem = $response.Content | ConvertFrom-Json
    $vegStatus = if ($menuItem.isVegetarian) { "🌱 Vegetarian" } else { "🍖 Non-Vegetarian" }
    Write-Host "✅ Success! Found menu item:"
    Write-Host "   Name: $($menuItem.name)"
    Write-Host "   Price: $($menuItem.price.ToString('C'))"
    Write-Host "   Category: $($menuItem.category)"
    Write-Host "   Type: $vegStatus"
    Write-Host "   Image: $($menuItem.imageUrl)"
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 5: Admin endpoint - Create new menu item
Write-Host "`n5. Testing POST /api/admin/menu (Admin endpoint)" -ForegroundColor Yellow
$newMenuItem = @{
    name = "Test Item"
    description = "A test menu item created via API"
    price = 9.99
    category = "Test"
    isAvailable = $true
    imageUrl = "https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=400"
    isVegetarian = $true
    preparationTimeMinutes = 10
} | ConvertTo-Json

$headers = @{
    "Content-Type" = "application/json"
    "adminKey" = "admin123"
}

try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/admin/menu" -Method POST -Body $newMenuItem -Headers $headers
    $createdItem = $response.Content | ConvertFrom-Json
    Write-Host "✅ Success! Created menu item with ID: $($createdItem.id)"
    Write-Host "   Name: $($createdItem.name)"
    Write-Host "   Price: $($createdItem.price.ToString('C'))"
} catch {
    Write-Host "❌ Error: $($_.Exception.Message)" -ForegroundColor Red
}

# Test 6: Test admin endpoint without key (should fail)
Write-Host "`n6. Testing POST /api/admin/menu without admin key (should fail)" -ForegroundColor Yellow
try {
    $response = Invoke-WebRequest -Uri "$baseUrl/api/admin/menu" -Method POST -Body $newMenuItem -Headers @{"Content-Type" = "application/json"}
    Write-Host "❌ Unexpected success - admin key validation failed!"
} catch {
    Write-Host "✅ Expected failure - admin key validation working: $($_.Exception.Message)" -ForegroundColor Green
}

Write-Host "`n=========================================" -ForegroundColor Green
Write-Host "API Testing Complete!" -ForegroundColor Green
