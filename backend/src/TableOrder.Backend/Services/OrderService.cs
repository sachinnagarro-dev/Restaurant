using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.Services;

public interface IOrderService
{
    Task<OrderResponse?> GetOrderByIdAsync(int id);
    Task<IEnumerable<OrderResponse>> GetOrdersAsync();
    Task<IEnumerable<OrderResponse>> GetOrdersByTableAsync(int tableId);
    Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request);
    Task<OrderResponse?> UpdateOrderStatusAsync(int id, OrderStatus status);
    Task<bool> DeleteOrderAsync(int id);
}

public class OrderService : IOrderService
{
    private readonly TableOrderDbContext _context;
    private readonly IHubContext<Hubs.OrderHub> _hubContext;

    public OrderService(TableOrderDbContext context, IHubContext<Hubs.OrderHub> hubContext)
    {
        _context = context;
        _hubContext = hubContext;
    }

    public async Task<OrderResponse?> GetOrderByIdAsync(int id)
    {
        var order = await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .FirstOrDefaultAsync(o => o.Id == id);

        return order == null ? null : MapToOrderResponse(order);
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersAsync()
    {
        var orders = await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToOrderResponse);
    }

    public async Task<IEnumerable<OrderResponse>> GetOrdersByTableAsync(int tableId)
    {
        var orders = await _context.Orders
            .Include(o => o.Table)
            .Include(o => o.OrderItems)
                .ThenInclude(oi => oi.MenuItem)
            .Where(o => o.TableId == tableId)
            .OrderByDescending(o => o.CreatedAt)
            .ToListAsync();

        return orders.Select(MapToOrderResponse);
    }

    public async Task<OrderResponse> CreateOrderAsync(CreateOrderRequest request)
    {
        var table = await _context.Tables.FindAsync(request.TableId);
        if (table == null)
            throw new ArgumentException("Table not found");

        var order = new Order
        {
            TableId = request.TableId,
            Status = OrderStatus.Received,
            SpecialInstructions = request.SpecialInstructions,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        decimal subTotal = 0;
        foreach (var item in request.OrderItems)
        {
            var menuItem = await _context.MenuItems.FindAsync(item.MenuItemId);
            if (menuItem == null || !menuItem.IsAvailable)
                throw new ArgumentException($"MenuItem {item.MenuItemId} not found or not available");

            var orderItem = new OrderItem
            {
                OrderId = order.Id,
                MenuItemId = item.MenuItemId,
                Quantity = item.Quantity,
                UnitPrice = menuItem.Price,
                SpecialInstructions = item.SpecialInstructions,
                CreatedAt = DateTime.UtcNow
            };

            order.OrderItems.Add(orderItem);
            subTotal += item.Quantity * menuItem.Price;
        }

        order.SubTotal = subTotal;
        order.TaxAmount = subTotal * 0.08m; // 8% tax
        order.TotalAmount = order.SubTotal + order.TaxAmount;

        _context.Orders.Add(order);
        await _context.SaveChangesAsync();

        // Send real-time update to kitchen
        await _hubContext.Clients.Group("Kitchen").SendAsync("NewOrder", order.Id);

        // Send confirmation to table
        await _hubContext.Clients.Group($"Table_{table.Number}").SendAsync("OrderConfirmed", order.Id);

        return await GetOrderByIdAsync(order.Id) ?? throw new InvalidOperationException("Failed to retrieve created order");
    }

    public async Task<OrderResponse?> UpdateOrderStatusAsync(int id, OrderStatus status)
    {
        var order = await _context.Orders
            .Include(o => o.Table)
            .FirstOrDefaultAsync(o => o.Id == id);

        if (order == null)
            return null;

        order.Status = status;
        order.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Send real-time update
        await _hubContext.Clients.Group("Kitchen").SendAsync("OrderStatusUpdated", id, status.ToString());
        await _hubContext.Clients.Group($"Table_{order.Table.Number}").SendAsync("OrderStatusUpdated", id, status.ToString());

        return await GetOrderByIdAsync(id);
    }

    public async Task<bool> DeleteOrderAsync(int id)
    {
        var order = await _context.Orders.FindAsync(id);
        if (order == null)
            return false;

        _context.Orders.Remove(order);
        await _context.SaveChangesAsync();

        return true;
    }

    private static OrderResponse MapToOrderResponse(Order order)
    {
        return new OrderResponse
        {
            Id = order.Id,
            TableId = order.TableId,
            TableNumber = order.Table.Number,
            Status = order.Status.ToString(),
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            SpecialInstructions = order.SpecialInstructions,
            CreatedAt = order.CreatedAt,
            OrderItems = order.OrderItems.Select(oi => new OrderItemResponse
            {
                Id = oi.Id,
                MenuItemId = oi.MenuItemId,
                MenuItemName = oi.MenuItem.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                SpecialInstructions = oi.SpecialInstructions
            }).ToList()
        };
    }
}
