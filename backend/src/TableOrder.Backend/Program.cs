using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Hubs;
using TableOrder.Backend.Models;
using TableOrder.Backend.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Add SignalR
builder.Services.AddSignalR();

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Entity Framework
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    // Use SQLite for development
    connectionString = "Data Source=tableorder.db";
}

// Check if connection string contains PostgreSQL indicators
if (connectionString.Contains("Host=") || connectionString.Contains("Server="))
{
    // Use PostgreSQL for production
    builder.Services.AddDbContext<TableOrderDbContext>(options =>
        options.UseNpgsql(connectionString));
}
else
{
    // Use SQLite for development
    builder.Services.AddDbContext<TableOrderDbContext>(options =>
        options.UseSqlite(connectionString));
}

// Add services
builder.Services.AddScoped<IOrderService, OrderService>();
builder.Services.AddScoped<IMenuService, MenuService>();
builder.Services.AddScoped<ITableService, TableService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IDatabaseSeeder, DatabaseSeeder>();

// Add payment gateway services
builder.Services.AddScoped<IPaymentGateway, MockPaymentGateway>();

var app = builder.Build();

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();
app.UseCors("AllowAll");

// Map SignalR hub
app.MapHub<OrderHub>("/orderHub");

// API endpoints
var api = app.MapGroup("/api");

// Menu endpoints are now handled by MenuController

// Table endpoints
api.MapGet("/tables", async (ITableService tableService) =>
{
    var tables = await tableService.GetTablesAsync();
    return Results.Ok(tables);
});

api.MapGet("/tables/{id:int}", async (int id, ITableService tableService) =>
{
    var table = await tableService.GetTableByIdAsync(id);
    return table == null ? Results.NotFound() : Results.Ok(table);
});

api.MapGet("/tables/number/{number:int}", async (int number, ITableService tableService) =>
{
    var table = await tableService.GetTableByNumberAsync(number);
    return table == null ? Results.NotFound() : Results.Ok(table);
});

api.MapPut("/tables/{id:int}/status/{status}", async (int id, string status, ITableService tableService) =>
{
    if (!Enum.TryParse<TableStatus>(status, out var tableStatus))
        return Results.BadRequest("Invalid status");

    var table = await tableService.UpdateTableStatusAsync(id, tableStatus);
    return table == null ? Results.NotFound() : Results.Ok(table);
});

// Order endpoints are now handled by OrderController

// Payment endpoints are now handled by PaymentController

app.MapControllers();

// Ensure database is created and seeded
using (var scope = app.Services.CreateScope())
{
    var context = scope.ServiceProvider.GetRequiredService<TableOrderDbContext>();
    var seeder = scope.ServiceProvider.GetRequiredService<IDatabaseSeeder>();
    
    context.Database.EnsureCreated();
    await seeder.SeedAsync();
}

app.Run();
