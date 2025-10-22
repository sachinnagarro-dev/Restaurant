using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.Controllers;

[ApiController]
[Route("api/admin")]
[Authorize(Roles = UserRole.Admin)]
public class AdminController : ControllerBase
{
    private readonly TableOrderDbContext _context;
    private readonly ILogger<AdminController> _logger;

    public AdminController(TableOrderDbContext context, ILogger<AdminController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all menu items for admin management
    /// </summary>
    [HttpGet("menu")]
    public async Task<ActionResult<IEnumerable<MenuItemResponse>>> GetMenuItems()
    {
        try
        {
            var menuItems = await _context.MenuItems
                .OrderBy(m => m.Category)
                .ThenBy(m => m.Name)
                .ToListAsync();

            var menuItemDtos = menuItems.Select(MapToMenuItemResponse).ToList();
            return Ok(menuItemDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving menu items for admin");
            return StatusCode(500, new { message = "An error occurred while retrieving menu items" });
        }
    }

    /// <summary>
    /// Create a new menu item
    /// </summary>
    [HttpPost("menu")]
    public async Task<ActionResult<MenuItemResponse>> CreateMenuItem([FromBody] CreateMenuItemDto request)
    {
        try
        {
            var menuItem = new MenuItem
            {
                Name = request.Name,
                Description = request.Description,
                Price = request.Price,
                Category = request.Category,
                IsAvailable = request.IsAvailable,
                IsVegetarian = request.IsVegetarian,
                ImageUrl = request.ImageUrl,
                PreparationTimeMinutes = request.PreparationTimeMinutes,
                RestaurantId = 1, // Default restaurant
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.MenuItems.Add(menuItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Created menu item: {menuItem.Name}");

            return CreatedAtAction(nameof(GetMenuItem), new { id = menuItem.Id }, MapToMenuItemResponse(menuItem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating menu item");
            return StatusCode(500, new { message = "An error occurred while creating the menu item" });
        }
    }

    /// <summary>
    /// Get a specific menu item
    /// </summary>
    [HttpGet("menu/{id}")]
    public async Task<ActionResult<MenuItemResponse>> GetMenuItem(int id)
    {
        try
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            return Ok(MapToMenuItemResponse(menuItem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error retrieving menu item {id}");
            return StatusCode(500, new { message = "An error occurred while retrieving the menu item" });
        }
    }

    /// <summary>
    /// Update a menu item
    /// </summary>
    [HttpPut("menu/{id}")]
    public async Task<ActionResult<MenuItemResponse>> UpdateMenuItem(int id, [FromBody] UpdateMenuItemDto request)
    {
        try
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            menuItem.Name = request.Name ?? menuItem.Name;
            menuItem.Description = request.Description ?? menuItem.Description;
            menuItem.Price = request.Price ?? menuItem.Price;
            menuItem.Category = request.Category ?? menuItem.Category;
            menuItem.IsAvailable = request.IsAvailable ?? menuItem.IsAvailable;
            menuItem.IsVegetarian = request.IsVegetarian ?? menuItem.IsVegetarian;
            menuItem.ImageUrl = request.ImageUrl ?? menuItem.ImageUrl;
            menuItem.PreparationTimeMinutes = request.PreparationTimeMinutes ?? menuItem.PreparationTimeMinutes;
            menuItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Updated menu item: {menuItem.Name}");

            return Ok(MapToMenuItemResponse(menuItem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error updating menu item {id}");
            return StatusCode(500, new { message = "An error occurred while updating the menu item" });
        }
    }

    /// <summary>
    /// Delete a menu item
    /// </summary>
    [HttpDelete("menu/{id}")]
    public async Task<ActionResult> DeleteMenuItem(int id)
    {
        try
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            _context.MenuItems.Remove(menuItem);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Deleted menu item: {menuItem.Name}");

            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error deleting menu item {id}");
            return StatusCode(500, new { message = "An error occurred while deleting the menu item" });
        }
    }

    /// <summary>
    /// Toggle menu item availability
    /// </summary>
    [HttpPatch("menu/{id}/availability")]
    public async Task<ActionResult<MenuItemResponse>> ToggleAvailability(int id, [FromBody] ToggleAvailabilityDto request)
    {
        try
        {
            var menuItem = await _context.MenuItems.FindAsync(id);
            if (menuItem == null)
            {
                return NotFound(new { message = "Menu item not found" });
            }

            menuItem.IsAvailable = request.IsAvailable;
            menuItem.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();

            _logger.LogInformation($"Toggled availability for menu item: {menuItem.Name} to {menuItem.IsAvailable}");

            return Ok(MapToMenuItemResponse(menuItem));
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, $"Error toggling availability for menu item {id}");
            return StatusCode(500, new { message = "An error occurred while updating availability" });
        }
    }

    /// <summary>
    /// Get daily analytics
    /// </summary>
    [HttpGet("analytics/daily")]
    public async Task<ActionResult<DailyAnalyticsDto>> GetDailyAnalytics([FromQuery] string? date = null)
    {
        try
        {
            var targetDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today 
                : DateTime.Parse(date).Date;

            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.CreatedAt.Date == targetDate)
                .ToListAsync();

            var totalOrders = orders.Count;
            var totalRevenue = orders.Sum(o => o.TotalAmount);
            var averageOrderValue = totalOrders > 0 ? totalRevenue / totalOrders : 0;

            var topItems = orders
                .SelectMany(o => o.OrderItems)
                .GroupBy(oi => new { oi.MenuItemId, oi.MenuItem.Name })
                .Select(g => new TopMenuItemDto
                {
                    MenuItemId = g.Key.MenuItemId,
                    MenuItemName = g.Key.Name,
                    QuantitySold = g.Sum(oi => oi.Quantity),
                    Revenue = g.Sum(oi => oi.Quantity * oi.UnitPrice)
                })
                .OrderByDescending(item => item.QuantitySold)
                .Take(10)
                .ToList();

            var analytics = new DailyAnalyticsDto
            {
                Date = targetDate.ToString("yyyy-MM-dd"),
                TotalOrders = totalOrders,
                TotalRevenue = totalRevenue,
                AverageOrderValue = averageOrderValue,
                TopItems = topItems
            };

            return Ok(analytics);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error retrieving daily analytics");
            return StatusCode(500, new { message = "An error occurred while retrieving analytics" });
        }
    }

    /// <summary>
    /// Export daily sales data
    /// </summary>
    [HttpGet("export/sales")]
    public async Task<ActionResult<IEnumerable<SalesExportDto>>> ExportDailySales([FromQuery] string? date = null)
    {
        try
        {
            var targetDate = string.IsNullOrEmpty(date) 
                ? DateTime.Today 
                : DateTime.Parse(date).Date;

            var orders = await _context.Orders
                .Include(o => o.Table)
                .Include(o => o.OrderItems)
                    .ThenInclude(oi => oi.MenuItem)
                .Where(o => o.CreatedAt.Date == targetDate)
                .ToListAsync();

            var exportData = orders
                .SelectMany(o => o.OrderItems.Select(oi => new SalesExportDto
                {
                    OrderId = o.Id,
                    TableNumber = o.Table.Number,
                    ItemName = oi.MenuItem.Name,
                    Quantity = oi.Quantity,
                    UnitPrice = oi.UnitPrice,
                    TotalPrice = oi.Quantity * oi.UnitPrice,
                    OrderDate = o.CreatedAt.ToString("yyyy-MM-dd HH:mm:ss"),
                    Status = o.Status.ToString()
                }))
                .ToList();

            return Ok(exportData);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error exporting sales data");
            return StatusCode(500, new { message = "An error occurred while exporting sales data" });
        }
    }

    private static MenuItemResponse MapToMenuItemResponse(MenuItem menuItem)
    {
        return new MenuItemResponse
        {
            Id = menuItem.Id,
            Name = menuItem.Name,
            Description = menuItem.Description,
            Price = menuItem.Price,
            Category = menuItem.Category,
            IsAvailable = menuItem.IsAvailable,
            IsVegetarian = menuItem.IsVegetarian,
            ImageUrl = menuItem.ImageUrl,
            PreparationTimeMinutes = menuItem.PreparationTimeMinutes,
            CreatedAt = menuItem.CreatedAt,
            UpdatedAt = menuItem.UpdatedAt
        };
    }
}

// DTOs for admin operations
public class CreateMenuItemDto
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public bool IsVegetarian { get; set; } = false;
    public string? ImageUrl { get; set; }
    public int PreparationTimeMinutes { get; set; } = 15;
}

public class UpdateMenuItemDto
{
    public string? Name { get; set; }
    public string? Description { get; set; }
    public decimal? Price { get; set; }
    public string? Category { get; set; }
    public bool? IsAvailable { get; set; }
    public bool? IsVegetarian { get; set; }
    public string? ImageUrl { get; set; }
    public int? PreparationTimeMinutes { get; set; }
}

public class ToggleAvailabilityDto
{
    public bool IsAvailable { get; set; }
}

public class DailyAnalyticsDto
{
    public string Date { get; set; } = string.Empty;
    public int TotalOrders { get; set; }
    public decimal TotalRevenue { get; set; }
    public decimal AverageOrderValue { get; set; }
    public List<TopMenuItemDto> TopItems { get; set; } = new();
}

public class TopMenuItemDto
{
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int QuantitySold { get; set; }
    public decimal Revenue { get; set; }
}

public class SalesExportDto
{
    public int OrderId { get; set; }
    public int TableNumber { get; set; }
    public string ItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string OrderDate { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
}
