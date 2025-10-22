# TableOrder.Backend

A .NET 8 Web API for restaurant table ordering system with real-time updates using SignalR.

## Features

- **RESTful API** with minimal API endpoints
- **Entity Framework Core** with SQLite (development) and PostgreSQL (production) support
- **SignalR Hub** for real-time order updates
- **Clean Architecture** with separated concerns
- **Docker Support** with containerization
- **Comprehensive Testing** with unit tests

## API Endpoints

### Menu
- `GET /api/menu` - Get all available menu items
- `GET /api/menu/categories` - Get all menu categories
- `GET /api/menu/{id}` - Get menu item by ID
- `GET /api/menu/category/{category}` - Get menu items by category

### Tables
- `GET /api/tables` - Get all tables
- `GET /api/tables/{id}` - Get table by ID
- `GET /api/tables/number/{number}` - Get table by number
- `PUT /api/tables/{id}/status/{status}` - Update table status

### Orders
- `GET /api/orders` - Get all orders
- `GET /api/orders/{id}` - Get order by ID
- `GET /api/orders/table/{tableId}` - Get orders by table
- `POST /api/orders` - Create new order
- `PUT /api/orders/{id}/status/{status}` - Update order status
- `DELETE /api/orders/{id}` - Delete order

### Payments
- `GET /api/payments/{id}` - Get payment by ID
- `GET /api/payments/order/{orderId}` - Get payments by order
- `POST /api/payments` - Create payment
- `PUT /api/payments/{id}/status/{status}` - Update payment status

### SignalR Hub
- **Hub URL**: `/orderHub`
- **Groups**: Kitchen, Table_{number}
- **Events**: NewOrder, OrderConfirmed, OrderStatusUpdated, PaymentStatusUpdated

## Quick Start

### Prerequisites
- .NET 8 SDK
- Docker (optional)

### Development Setup

1. **Clone and navigate to backend**
   ```bash
   cd backend
   ```

2. **Restore dependencies**
   ```bash
   dotnet restore
   ```

3. **Run the application**
   ```bash
   dotnet run --project src/TableOrder.Backend
   ```

4. **Access the API**
   - API: `https://localhost:5001`
   - Swagger UI: `https://localhost:5001/swagger`

### Docker Setup

1. **Build and run with Docker Compose**
   ```bash
   docker-compose up --build
   ```

2. **Access the application**
   - API: `http://localhost:5000`
   - Swagger UI: `http://localhost:5000/swagger`

### Database Configuration

#### Development (SQLite)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=tableorder.db"
  }
}
```

#### Production (PostgreSQL)
```json
{
  "ConnectionStrings": {
    "DefaultConnection": "Host=localhost;Database=tableorder;Username=postgres;Password=your_password"
  }
}
```

## Testing

### Run Unit Tests
```bash
dotnet test
```

### Run Tests with Coverage
```bash
dotnet test --collect:"XPlat Code Coverage"
```

## Project Structure

```
src/TableOrder.Backend/
├── Data/
│   └── TableOrderDbContext.cs          # EF Core DbContext
├── DTOs/                               # Data Transfer Objects
│   ├── CreateOrderRequest.cs
│   ├── OrderResponse.cs
│   ├── MenuItemResponse.cs
│   └── PaymentRequest.cs
├── Hubs/
│   └── OrderHub.cs                     # SignalR Hub
├── Models/                             # Domain Models
│   ├── Restaurant.cs
│   ├── Table.cs
│   ├── MenuItem.cs
│   ├── Order.cs
│   ├── OrderItem.cs
│   └── Payment.cs
├── Services/                           # Business Logic
│   ├── OrderService.cs
│   ├── MenuService.cs
│   ├── TableService.cs
│   └── PaymentService.cs
├── Program.cs                          # Application entry point
└── appsettings.json                    # Configuration
```

## Environment Variables

- `ASPNETCORE_ENVIRONMENT` - Environment (Development/Production)
- `ConnectionStrings__DefaultConnection` - Database connection string

## SignalR Client Usage

### JavaScript Client
```javascript
const connection = new signalR.HubConnectionBuilder()
    .withUrl("/orderHub")
    .build();

// Join kitchen group
await connection.invoke("JoinKitchenGroup");

// Join table group
await connection.invoke("JoinTableGroup", tableNumber);

// Listen for events
connection.on("NewOrder", (orderId) => {
    console.log("New order received:", orderId);
});

connection.on("OrderStatusUpdated", (orderId, status) => {
    console.log("Order status updated:", orderId, status);
});
```

## Deployment

### Docker Production
```bash
docker-compose -f docker-compose.production.yml up -d
```

### Manual Deployment
1. Build the application
   ```bash
   dotnet publish -c Release -o ./publish
   ```

2. Deploy to server and run
   ```bash
   dotnet ./publish/TableOrder.Backend.dll
   ```

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Add tests for new functionality
5. Run tests to ensure everything passes
6. Submit a pull request

## License

This project is licensed under the MIT License.
