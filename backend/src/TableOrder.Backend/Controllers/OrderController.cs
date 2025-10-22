using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Hubs;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrderController : ControllerBase
{
    private readonly TableOrderDbContext _context;
    private readonly IHubContext<OrderHub> _hubContext;
    private readonly ILogger<OrderController> _logger;

    public OrderController(
        TableOrderDbContext context,
        IHubContext<OrderHub> hubContext,
        ILogger<OrderController> logger)
    {
        _context = context;
        _hubContext = hubContext;
        _logger = logger;
    }

    /// <summary>
    /// Create a new order
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<OrderResponseDto>> CreateOrder([FromBody] CreateOrderDto request)
    {
        try
        {
            // Validate table exists
            var table = await _context.Tables
                .FirstOrDefaultAsync(t => t.Id == request.TableId);
            
            if (table == null)
            {
                return BadRequest(new { message = "Table not found" });
            }

            // Validate menu items exist and are available
            var menuItemIds = request.Items.Select(i => i.MenuItemId).ToList();
            var menuItems = await _context.MenuItems
                .Where(m => menuItemIds.Contains(m.Id))
                .ToListAsync();

            if (menuItems.Count != menuItemIds.Count)
            {
                return BadRequest(new { message = "One or more menu items not found" });
            }

            var unavailableItems = menuItems.Where(m => !m.IsAvailable).ToList();
            if (unavailableItems.Any())
            {
                var unavailableNames = unavailableItems.Select(m => m.Name).ToList();
                return BadRequest(new { message = $"The following items are not available: {string.Join(", ", unavailableNames)}" });
            }

            // Create order
            var order = new Order
            {
                TableId = request.TableId,
                Status = OrderStatus.Received,
                SpecialInstructions = request.Remarks,
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            // Calculate totals
            decimal subTotal = 0;
            foreach (var item in request.Items)
            {
                var menuItem = menuItems.First(m => m.Id == item.MenuItemId);
                var orderItem = new OrderItem
                {
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

            // Broadcast order creation to SignalR clients
            await BroadcastOrderEvent("OrderCreated", new
            {
                OrderId = order.Id,
                TableId = order.TableId,
                TableNumber = table.Number,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ItemCount = order.OrderItems.Count,
                CreatedAt = order.CreatedAt
            });

            _logger.LogInformation($"Order {order.Id} created for table {table.Number}");

            return CreatedAtAction(nameof(GetOrder), new { id = order.Id }, MapToOrderResponse(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating order");
            return StatusCode(500, new { message = "An error occurred while creating the order" });
        }
    }

    /// <summary>
    /// Get order details by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<OrderResponseDto>> GetOrder(int id)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            return Ok(MapToOrderResponse(order));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving order {id}");
            return StatusCode(500, new { message = "An error occurred while retrieving the order" });
        }
    }

    /// <summary>
    /// Update order status
    /// </summary>
    [HttpPut("{id}/status")]
    public async Task<ActionResult<OrderResponseDto>> UpdateOrderStatus(int id, [FromBody] UpdateOrderStatusDto request)
    {
        try
        {
            var order = await _context.Orders
                .Include(o => o.Table)
                .FirstOrDefaultAsync(o => o.Id == id);

            if (order == null)
            {
                return NotFound(new { message = "Order not found" });
            }

            var oldStatus = order.Status;
            order.Status = request.Status;
            order.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            // Broadcast status update to SignalR clients
            await BroadcastOrderEvent("OrderStatusUpdated", new
            {
                OrderId = order.Id,
                TableId = order.TableId,
                TableNumber = order.Table.Number,
                OldStatus = oldStatus.ToString(),
                NewStatus = order.Status.ToString(),
                UpdatedAt = order.UpdatedAt
            });

            _logger.LogInformation($"Order {order.Id} status updated from {oldStatus} to {order.Status}");

            // Get updated order with items for response
            var updatedOrder = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .FirstAsync(o => o.Id == id);

            return Ok(MapToOrderResponse(updatedOrder));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating order {id} status");
            return StatusCode(500, new { message = "An error occurred while updating the order status" });
        }
    }

    /// <summary>
    /// Get all orders
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetAllOrders()
    {
        try
        {
            var orders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderSummaries = orders.Select(order => new OrderSummaryDto
            {
                Id = order.Id,
                TableId = order.TableId,
                TableNumber = order.Table.Number,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ItemCount = order.OrderItems.Sum(oi => oi.Quantity),
                CreatedAt = order.CreatedAt
            });

            return Ok(orderSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving all orders");
            return StatusCode(500, new { message = "An error occurred while retrieving orders" });
        }
    }

    /// <summary>
    /// Get orders by table ID
    /// </summary>
    [HttpGet("table/{tableId}")]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByTable(int tableId)
    {
        try
        {
            var orders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                .Where(o => o.TableId == tableId)
                .OrderByDescending(o => o.CreatedAt)
                .ToListAsync();

            var orderSummaries = orders.Select(order => new OrderSummaryDto
            {
                Id = order.Id,
                TableId = order.TableId,
                TableNumber = order.Table.Number,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ItemCount = order.OrderItems.Sum(oi => oi.Quantity),
                CreatedAt = order.CreatedAt
            });

            return Ok(orderSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving orders for table {tableId}");
            return StatusCode(500, new { message = "An error occurred while retrieving orders for the table" });
        }
    }

    /// <summary>
    /// Get orders by status
    /// </summary>
    [HttpGet("status/{status}")]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>> GetOrdersByStatus(string status)
    {
        try
        {
            if (!Enum.TryParse<OrderStatus>(status, true, out var orderStatus))
            {
                return BadRequest(new { message = "Invalid status" });
            }

            var orders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                .Where(o => o.Status == orderStatus)
                .OrderBy(o => o.CreatedAt)
                .ToListAsync();

            var orderSummaries = orders.Select(order => new OrderSummaryDto
            {
                Id = order.Id,
                TableId = order.TableId,
                TableNumber = order.Table.Number,
                Status = order.Status.ToString(),
                TotalAmount = order.TotalAmount,
                ItemCount = order.OrderItems.Sum(oi => oi.Quantity),
                CreatedAt = order.CreatedAt
            });

            return Ok(orderSummaries);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving orders with status {status}");
            return StatusCode(500, new { message = "An error occurred while retrieving orders by status" });
        }
    }

    private async Task BroadcastOrderEvent(string eventName, object data)
    {
        try
        {
            // Broadcast to kitchen group
            await _hubContext.Clients.Group("Kitchen").SendAsync(eventName, data);

            // Broadcast to specific table group
            if (data is not null)
            {
                var dataType = data.GetType();
                var tableNumberProperty = dataType.GetProperty("TableNumber");
                if (tableNumberProperty != null)
                {
                    var tableNumber = tableNumberProperty.GetValue(data);
                    if (tableNumber != null)
                    {
                        await _hubContext.Clients.Group($"Table_{tableNumber}").SendAsync(eventName, data);
                    }
                }
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error broadcasting SignalR event: {eventName}");
        }
    }

    private static OrderResponseDto MapToOrderResponse(Order order)
    {
        return new OrderResponseDto
        {
            Id = order.Id,
            TableId = order.TableId,
            TableNumber = order.Table.Number,
            Status = order.Status.ToString(),
            SubTotal = order.SubTotal,
            TaxAmount = order.TaxAmount,
            TotalAmount = order.TotalAmount,
            Remarks = order.SpecialInstructions,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.OrderItems.Select(oi => new OrderItemResponseDto
            {
                Id = oi.Id,
                MenuItemId = oi.MenuItemId,
                MenuItemName = oi.MenuItem.Name,
                Quantity = oi.Quantity,
                UnitPrice = oi.UnitPrice,
                TotalPrice = oi.Quantity * oi.UnitPrice,
                SpecialInstructions = oi.SpecialInstructions
            }).ToList()
        };
    }
}
