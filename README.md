# TableOrder Restaurant System

A comprehensive restaurant ordering system with table-side ordering, real-time kitchen management, and integrated payment processing.

## 🏗️ Architecture Overview

- **Frontend**: React + TypeScript (Table PWA, Kitchen Display, Admin Panel)
- **Backend**: .NET 8 Web API + SignalR for real-time updates
- **Database**: SQLite (development) / PostgreSQL (production)
- **Infrastructure**: Docker containerization with Redis caching
- **Communication**: WiFi network with offline-first strategy

## 📁 Repository Structure

```
RestaurantApp/
├── backend/                    # .NET 8 Web API backend
│   ├── src/
│   │   ├── TableOrder.Api/     # Web API project
│   │   ├── TableOrder.Core/    # Domain models
│   │   ├── TableOrder.Infrastructure/ # EF Core & services
│   │   └── TableOrder.Application/ # Business logic
│   ├── tests/                  # Unit & integration tests
│   └── TableOrder.sln         # Solution file
├── frontend/                   # React applications
│   ├── table-app/             # Table ordering PWA
│   ├── kitchen-app/           # Kitchen management
│   ├── admin-app/             # Admin panel
│   └── shared/                # Shared components
├── infrastructure/            # Docker & deployment
│   ├── docker/               # Docker configurations
│   ├── database/             # Migrations & seed data
│   └── nginx/                # Reverse proxy config
├── docs/                     # Documentation
└── scripts/                  # Setup & deployment scripts
```

## 🚀 Quick Start

### Prerequisites

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8.0)
- [Node.js 18+](https://nodejs.org/)
- [Docker Desktop](https://www.docker.com/products/docker-desktop)
- [Git](https://git-scm.com/)

### 1. Clone Repository

```bash
git clone <repository-url>
cd RestaurantApp
```

### 2. Backend Setup

```bash
cd backend

# Create solution and projects
dotnet new sln -n TableOrder
cd src

# Create projects
dotnet new webapi -n TableOrder.Api
dotnet new classlib -n TableOrder.Core
dotnet new classlib -n TableOrder.Infrastructure
dotnet new classlib -n TableOrder.Application

# Add to solution
cd ..
dotnet sln add src/TableOrder.Api/TableOrder.Api.csproj
dotnet sln add src/TableOrder.Core/TableOrder.Core.csproj
dotnet sln add src/TableOrder.Infrastructure/TableOrder.Infrastructure.csproj
dotnet sln add src/TableOrder.Application/TableOrder.Application.csproj

# Add project references
cd src
dotnet add TableOrder.Api/TableOrder.Api.csproj reference TableOrder.Application/TableOrder.Application.csproj
dotnet add TableOrder.Application/TableOrder.Application.csproj reference TableOrder.Core/TableOrder.Core.csproj
dotnet add TableOrder.Infrastructure/TableOrder.Infrastructure.csproj reference TableOrder.Core/TableOrder.Core.csproj
dotnet add TableOrder.Application/TableOrder.Application.csproj reference TableOrder.Infrastructure/TableOrder.Infrastructure.csproj

# Install packages
cd TableOrder.Api
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Microsoft.AspNetCore.SignalR
dotnet add package Microsoft.AspNetCore.Authentication.JwtBearer
dotnet add package AutoMapper.Extensions.Microsoft.DependencyInjection

cd ../TableOrder.Infrastructure
dotnet add package Microsoft.EntityFrameworkCore.Sqlite
dotnet add package Microsoft.EntityFrameworkCore.Tools
dotnet add package Npgsql.EntityFrameworkCore.PostgreSQL
dotnet add package StackExchange.Redis

# Restore packages
cd ../..
dotnet restore
```

### 3. Frontend Setup

```bash
cd ../frontend

# Create table ordering app
npx create-react-app table-app --template typescript
cd table-app
npm install @mui/material @emotion/react @emotion/styled
npm install @microsoft/signalr
npm install @tanstack/react-query
npm install @types/node

# Create kitchen display app
cd ..
npx create-react-app kitchen-app --template typescript
cd kitchen-app
npm install @mui/material @emotion/react @emotion/styled
npm install @microsoft/signalr
npm install @tanstack/react-query

# Create admin panel
cd ..
npx create-react-app admin-app --template typescript
cd admin-app
npm install @mui/material @emotion/react @emotion/styled
npm install @microsoft/signalr
npm install @tanstack/react-query
npm install recharts  # for analytics dashboard

# Create shared components library
cd ..
mkdir shared
cd shared
npm init -y
npm install @mui/material @emotion/react @emotion/styled
npm install @types/react @types/react-dom
```

### 4. Infrastructure Setup

```bash
cd ../infrastructure

# Create Docker configuration
mkdir docker
cd docker

# Create Dockerfile for API
cat > Dockerfile.api << 'EOF'
FROM mcr.microsoft.com/dotnet/aspnet:8.0 AS base
WORKDIR /app
EXPOSE 80
EXPOSE 443

FROM mcr.microsoft.com/dotnet/sdk:8.0 AS build
WORKDIR /src
COPY ["TableOrder.Api/TableOrder.Api.csproj", "TableOrder.Api/"]
COPY ["TableOrder.Application/TableOrder.Application.csproj", "TableOrder.Application/"]
COPY ["TableOrder.Infrastructure/TableOrder.Infrastructure.csproj", "TableOrder.Infrastructure/"]
COPY ["TableOrder.Core/TableOrder.Core.csproj", "TableOrder.Core/"]
RUN dotnet restore "TableOrder.Api/TableOrder.Api.csproj"
COPY . .
WORKDIR "/src/TableOrder.Api"
RUN dotnet build "TableOrder.Api.csproj" -c Release -o /app/build

FROM build AS publish
RUN dotnet publish "TableOrder.Api.csproj" -c Release -o /app/publish

FROM base AS final
WORKDIR /app
COPY --from=publish /app/publish .
ENTRYPOINT ["dotnet", "TableOrder.Api.dll"]
EOF

# Create Dockerfile for frontend
cat > Dockerfile.frontend << 'EOF'
FROM node:18-alpine AS build
WORKDIR /app
COPY package*.json ./
RUN npm ci --only=production
COPY . .
RUN npm run build

FROM nginx:alpine
COPY --from=build /app/build /usr/share/nginx/html
COPY nginx.conf /etc/nginx/nginx.conf
EXPOSE 80
CMD ["nginx", "-g", "daemon off;"]
EOF

# Create docker-compose.yml
cat > docker-compose.yml << 'EOF'
version: '3.8'

services:
  api:
    build:
      context: ../../backend/src
      dockerfile: ../../infrastructure/docker/Dockerfile.api
    ports:
      - "5000:80"
    environment:
      - ASPNETCORE_ENVIRONMENT=Development
      - ConnectionStrings__DefaultConnection=Host=postgres;Database=tableorder;Username=postgres;Password=password
      - Redis__ConnectionString=redis:6379
    depends_on:
      - postgres
      - redis

  postgres:
    image: postgres:15-alpine
    environment:
      - POSTGRES_DB=tableorder
      - POSTGRES_USER=postgres
      - POSTGRES_PASSWORD=password
    ports:
      - "5432:5432"
    volumes:
      - postgres_data:/var/lib/postgresql/data

  redis:
    image: redis:7-alpine
    ports:
      - "6379:6379"

  frontend:
    build:
      context: ../../frontend/table-app
      dockerfile: ../../infrastructure/docker/Dockerfile.frontend
    ports:
      - "3000:80"
    depends_on:
      - api

volumes:
  postgres_data:
EOF

# Create nginx configuration
mkdir nginx
cat > nginx/nginx.conf << 'EOF'
events {
    worker_connections 1024;
}

http {
    upstream api {
        server api:80;
    }

    server {
        listen 80;
        
        location /api/ {
            proxy_pass http://api/;
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
        }

        location /hub/ {
            proxy_pass http://api/hub/;
            proxy_set_header Upgrade $http_upgrade;
            proxy_set_header Connection "upgrade";
            proxy_set_header Host $host;
            proxy_set_header X-Real-IP $remote_addr;
        }

        location / {
            root /usr/share/nginx/html;
            index index.html index.htm;
            try_files $uri $uri/ /index.html;
        }
    }
}
EOF
```

### 5. Database Setup

```bash
cd ../database
mkdir migrations seed-data

# Create initial migration script
cat > migrations/001_initial_schema.sql << 'EOF'
-- Tables
CREATE TABLE Tables (
    Id SERIAL PRIMARY KEY,
    Number INTEGER NOT NULL UNIQUE,
    Capacity INTEGER NOT NULL,
    Status VARCHAR(20) DEFAULT 'Available',
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE MenuItems (
    Id SERIAL PRIMARY KEY,
    Name VARCHAR(100) NOT NULL,
    Description TEXT,
    Price DECIMAL(10,2) NOT NULL,
    Category VARCHAR(50) NOT NULL,
    IsAvailable BOOLEAN DEFAULT TRUE,
    ImageUrl VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE Orders (
    Id SERIAL PRIMARY KEY,
    TableId INTEGER REFERENCES Tables(Id),
    Status VARCHAR(20) DEFAULT 'Pending',
    TotalAmount DECIMAL(10,2) NOT NULL,
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP,
    UpdatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

CREATE TABLE OrderItems (
    Id SERIAL PRIMARY KEY,
    OrderId INTEGER REFERENCES Orders(Id),
    MenuItemId INTEGER REFERENCES MenuItems(Id),
    Quantity INTEGER NOT NULL,
    UnitPrice DECIMAL(10,2) NOT NULL,
    SpecialInstructions TEXT
);

CREATE TABLE Payments (
    Id SERIAL PRIMARY KEY,
    OrderId INTEGER REFERENCES Orders(Id),
    Amount DECIMAL(10,2) NOT NULL,
    PaymentMethod VARCHAR(50) NOT NULL,
    Status VARCHAR(20) DEFAULT 'Pending',
    TransactionId VARCHAR(255),
    CreatedAt TIMESTAMP DEFAULT CURRENT_TIMESTAMP
);

-- Indexes
CREATE INDEX IX_Orders_TableId ON Orders(TableId);
CREATE INDEX IX_Orders_Status ON Orders(Status);
CREATE INDEX IX_OrderItems_OrderId ON OrderItems(OrderId);
CREATE INDEX IX_Payments_OrderId ON Payments(OrderId);
EOF

# Create seed data
cat > seed-data/seed_data.sql << 'EOF'
-- Sample Tables
INSERT INTO Tables (Number, Capacity) VALUES 
(1, 2), (2, 4), (3, 6), (4, 2), (5, 8), (6, 4), (7, 2), (8, 6);

-- Sample Menu Items
INSERT INTO MenuItems (Name, Description, Price, Category, IsAvailable) VALUES
('Margherita Pizza', 'Classic tomato, mozzarella, basil', 12.99, 'Pizza', TRUE),
('Pepperoni Pizza', 'Tomato, mozzarella, pepperoni', 14.99, 'Pizza', TRUE),
('Caesar Salad', 'Romaine lettuce, parmesan, croutons', 8.99, 'Salad', TRUE),
('Chicken Wings', 'Spicy buffalo wings with ranch', 9.99, 'Appetizer', TRUE),
('Pasta Carbonara', 'Creamy pasta with bacon and egg', 13.99, 'Pasta', TRUE),
('Grilled Salmon', 'Fresh salmon with vegetables', 18.99, 'Main Course', TRUE),
('Chocolate Cake', 'Rich chocolate cake with ice cream', 6.99, 'Dessert', TRUE),
('Coca Cola', 'Refreshing soft drink', 2.99, 'Beverage', TRUE);
EOF
```

## 🛠️ Development Commands

### Backend Development

```bash
cd backend

# Run API in development mode
dotnet run --project src/TableOrder.Api

# Run tests
dotnet test

# Add new migration
dotnet ef migrations add <MigrationName> --project src/TableOrder.Infrastructure --startup-project src/TableOrder.Api

# Update database
dotnet ef database update --project src/TableOrder.Infrastructure --startup-project src/TableOrder.Api
```

### Frontend Development

```bash
cd frontend

# Table app
cd table-app
npm start

# Kitchen app
cd ../kitchen-app
npm start

# Admin app
cd ../admin-app
npm start
```

### Docker Development

```bash
cd infrastructure/docker

# Start all services
docker-compose up -d

# View logs
docker-compose logs -f api

# Stop services
docker-compose down
```

## 🧪 Testing

### Backend Tests

```bash
cd backend

# Unit tests
dotnet test tests/TableOrder.UnitTests

# Integration tests
dotnet test tests/TableOrder.IntegrationTests

# Coverage report
dotnet test --collect:"XPlat Code Coverage"
```

### Frontend Tests

```bash
cd frontend

# Run all tests
npm test -- --coverage

# E2E tests (when implemented)
npm run test:e2e
```

## 📦 Production Deployment

### Docker Production Build

```bash
# Build production images
docker build -f infrastructure/docker/Dockerfile.api -t tableorder-api:latest backend/src
docker build -f infrastructure/docker/Dockerfile.frontend -t tableorder-frontend:latest frontend/table-app

# Deploy with docker-compose
cd infrastructure/docker
docker-compose -f docker-compose.prod.yml up -d
```

### Environment Variables

Create `.env` files for different environments:

```bash
# .env.development
ASPNETCORE_ENVIRONMENT=Development
ConnectionStrings__DefaultConnection=Data Source=tableorder.db
Redis__ConnectionString=localhost:6379

# .env.production
ASPNETCORE_ENVIRONMENT=Production
ConnectionStrings__DefaultConnection=Host=postgres;Database=tableorder;Username=postgres;Password=your_password
Redis__ConnectionString=redis:6379
```

## 📚 Documentation

- [Architecture Documentation](./ARCHITECTURE.md)
- [API Documentation](./docs/api/README.md)
- [Deployment Guide](./docs/deployment/README.md)
- [User Guides](./docs/user-guides/README.md)

## 🤝 Contributing

1. Fork the repository
2. Create a feature branch (`git checkout -b feature/amazing-feature`)
3. Commit your changes (`git commit -m 'Add amazing feature'`)
4. Push to the branch (`git push origin feature/amazing-feature`)
5. Open a Pull Request

## 📄 License

This project is licensed under the MIT License - see the [LICENSE](LICENSE) file for details.

## 🆘 Support

For support and questions:
- Create an issue in the repository
- Check the documentation in the `docs/` folder
- Review the architecture documentation

---

**Happy coding! 🍕🍔🍰**
