# Menu API Documentation

## Overview

The Menu API provides endpoints for retrieving menu items and administrative functions for managing the restaurant menu. The API returns JSON data optimized for tablet UI consumption.

## Endpoints

### Public Endpoints

#### GET /api/menu
Returns all menu items grouped by categories.

**Response Format:**
```json
{
  "categories": [
    {
      "name": "Pizza",
      "items": [
        {
          "id": 1,
          "name": "Margherita Pizza",
          "description": "Classic tomato sauce, fresh mozzarella, and basil",
          "price": 12.99,
          "category": "Pizza",
          "isAvailable": true,
          "imageUrl": "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?w=400",
          "isVegetarian": true,
          "preparationTimeMinutes": 20
        }
      ]
    }
  ],
  "totalItems": 24
}
```

#### GET /api/menu/categories
Returns all available menu categories.

**Response Format:**
```json
[
  "Pizza",
  "Pasta",
  "Main Course",
  "Salad",
  "Appetizer",
  "Dessert",
  "Beverage"
]
```

#### GET /api/menu/category/{category}
Returns menu items for a specific category.

**Example:** `/api/menu/category/Pizza`

**Response Format:**
```json
[
  {
    "id": 1,
    "name": "Margherita Pizza",
    "description": "Classic tomato sauce, fresh mozzarella, and basil",
    "price": 12.99,
    "category": "Pizza",
    "isAvailable": true,
    "imageUrl": "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?w=400",
    "isVegetarian": true,
    "preparationTimeMinutes": 20
  }
]
```

#### GET /api/menu/{id}
Returns a specific menu item by ID.

**Response Format:**
```json
{
  "id": 1,
  "name": "Margherita Pizza",
  "description": "Classic tomato sauce, fresh mozzarella, and basil",
  "price": 12.99,
  "category": "Pizza",
  "isAvailable": true,
  "imageUrl": "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?w=400",
  "isVegetarian": true,
  "preparationTimeMinutes": 20
}
```

### Admin Endpoints

All admin endpoints require an `adminKey` header for authentication.

#### POST /api/admin/menu
Creates a new menu item.

**Headers:**
- `Content-Type: application/json`
- `adminKey: admin123` (default admin key)

**Request Body:**
```json
{
  "name": "New Menu Item",
  "description": "Description of the new item",
  "price": 12.99,
  "category": "Pizza",
  "isAvailable": true,
  "imageUrl": "https://example.com/image.jpg",
  "isVegetarian": true,
  "preparationTimeMinutes": 20
}
```

**Response:** Returns the created menu item with assigned ID.

#### PUT /api/admin/menu/{id}
Updates an existing menu item.

**Headers:**
- `Content-Type: application/json`
- `adminKey: admin123`

**Request Body:** Same as POST request.

**Response:** Returns the updated menu item.

#### DELETE /api/admin/menu/{id}
Deletes a menu item.

**Headers:**
- `adminKey: admin123`

**Response:** 204 No Content on success.

## Database Seeding

The application automatically seeds the database with sample menu items on first run. The seeder includes:

- **1 Restaurant** with basic information
- **10 Tables** (numbered 1-10 with varying capacities)
- **24 Menu Items** across 7 categories:
  - Pizza (4 items)
  - Pasta (3 items)
  - Main Course (3 items)
  - Salad (3 items)
  - Appetizer (3 items)
  - Dessert (3 items)
  - Beverage (3 items)

### Sample Menu Items Include:

**Pizza Category:**
- Margherita Pizza (Veg) - $12.99
- Pepperoni Pizza (Non-Veg) - $14.99
- BBQ Chicken Pizza (Non-Veg) - $16.99
- Veggie Supreme Pizza (Veg) - $15.99

**Pasta Category:**
- Pasta Carbonara (Non-Veg) - $13.99
- Spaghetti Marinara (Veg) - $11.99
- Chicken Alfredo (Non-Veg) - $15.99

**Main Course:**
- Grilled Salmon (Non-Veg) - $18.99
- Grilled Chicken Breast (Non-Veg) - $16.99
- Veggie Burger (Veg) - $12.99

And more items across all categories...

## Configuration

### Admin Key
The admin key is configured in `appsettings.json`:

```json
{
  "AdminKey": "admin123"
}
```

For production, change this to a secure key.

### Database
The application uses SQLite for development and can be configured for PostgreSQL in production.

## Testing

Use the provided PowerShell test script:

```powershell
.\test-endpoints.ps1
```

This script tests all endpoints and demonstrates the API functionality.

## Migration

The application includes an EF Core migration for the new `IsVegetarian` column:

```bash
dotnet ef migrations add AddVegetarianFlag --project src/TableOrder.Backend
```

## Tablet UI Integration

The JSON response format is optimized for tablet UI consumption with:

- **High-resolution images** from Unsplash (400px width)
- **Vegetarian/Non-vegetarian flags** for dietary preferences
- **Preparation time** for kitchen planning
- **Availability status** for real-time menu updates
- **Category grouping** for organized display

## Error Handling

The API returns appropriate HTTP status codes:

- `200 OK` - Successful GET requests
- `201 Created` - Successful POST requests
- `204 No Content` - Successful DELETE requests
- `400 Bad Request` - Invalid request data
- `401 Unauthorized` - Missing or invalid admin key
- `404 Not Found` - Resource not found

## Example Usage

### Frontend Integration (JavaScript)

```javascript
// Get all menu items
fetch('/api/menu')
  .then(response => response.json())
  .then(data => {
    data.categories.forEach(category => {
      console.log(`${category.name}: ${category.items.length} items`);
      category.items.forEach(item => {
        const vegIcon = item.isVegetarian ? '🌱' : '🍖';
        console.log(`  ${vegIcon} ${item.name} - $${item.price}`);
      });
    });
  });

// Create new menu item (Admin)
fetch('/api/admin/menu', {
  method: 'POST',
  headers: {
    'Content-Type': 'application/json',
    'adminKey': 'admin123'
  },
  body: JSON.stringify({
    name: 'New Pizza',
    description: 'A delicious new pizza',
    price: 14.99,
    category: 'Pizza',
    isAvailable: true,
    imageUrl: 'https://example.com/pizza.jpg',
    isVegetarian: false,
    preparationTimeMinutes: 25
  })
})
.then(response => response.json())
.then(data => console.log('Created:', data));
```
