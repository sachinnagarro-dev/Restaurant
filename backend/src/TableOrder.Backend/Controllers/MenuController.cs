using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MenuController : ControllerBase
{
    private readonly TableOrderDbContext _context;
    private readonly IConfiguration _configuration;

    public MenuController(TableOrderDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Get all menu items grouped by categories
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<MenuResponse>> GetMenu()
    {
        var menuItems = await _context.MenuItems
            .Where(m => m.IsAvailable)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();

        var categories = menuItems
            .GroupBy(m => m.Category)
            .Select(g => new CategoryResponse
            {
                Name = g.Key,
                Items = g.Select(item => new MenuItemResponse
                {
                    Id = item.Id,
                    Name = item.Name,
                    Description = item.Description,
                    Price = item.Price,
                    Category = item.Category,
                    IsAvailable = item.IsAvailable,
                    ImageUrl = item.ImageUrl,
                    IsVegetarian = item.IsVegetarian,
                    PreparationTimeMinutes = item.PreparationTimeMinutes
                }).ToList()
            })
            .ToList();

        return Ok(new MenuResponse
        {
            Categories = categories,
            TotalItems = menuItems.Count
        });
    }

    /// <summary>
    /// Get menu items by category
    /// </summary>
    [HttpGet("category/{category}")]
    public async Task<ActionResult<IEnumerable<MenuItemResponse>>> GetMenuByCategory(string category)
    {
        var menuItems = await _context.MenuItems
            .Where(m => m.IsAvailable && m.Category.ToLower() == category.ToLower())
            .OrderBy(m => m.Name)
            .ToListAsync();

        var response = menuItems.Select(item => new MenuItemResponse
        {
            Id = item.Id,
            Name = item.Name,
            Description = item.Description,
            Price = item.Price,
            Category = item.Category,
            IsAvailable = item.IsAvailable,
            ImageUrl = item.ImageUrl,
            IsVegetarian = item.IsVegetarian,
            PreparationTimeMinutes = item.PreparationTimeMinutes
        });

        return Ok(response);
    }

    /// <summary>
    /// Get all available categories
    /// </summary>
    [HttpGet("categories")]
    public async Task<ActionResult<IEnumerable<string>>> GetCategories()
    {
        var categories = await _context.MenuItems
            .Where(m => m.IsAvailable)
            .Select(m => m.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();

        return Ok(categories);
    }

    /// <summary>
    /// Get menu item by ID
    /// </summary>
    [HttpGet("{id}")]
    public async Task<ActionResult<MenuItemResponse>> GetMenuItem(int id)
    {
        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == id && m.IsAvailable);

        if (menuItem == null)
            return NotFound();

        var response = new MenuItemResponse
        {
            Id = menuItem.Id,
            Name = menuItem.Name,
            Description = menuItem.Description,
            Price = menuItem.Price,
            Category = menuItem.Category,
            IsAvailable = menuItem.IsAvailable,
            ImageUrl = menuItem.ImageUrl,
            IsVegetarian = menuItem.IsVegetarian,
            PreparationTimeMinutes = menuItem.PreparationTimeMinutes
        };

        return Ok(response);
    }
}

[ApiController]
[Route("api/admin/[controller]")]
public class AdminMenuController : ControllerBase
{
    private readonly TableOrderDbContext _context;
    private readonly IConfiguration _configuration;

    public AdminMenuController(TableOrderDbContext context, IConfiguration configuration)
    {
        _context = context;
        _configuration = configuration;
    }

    /// <summary>
    /// Create or update a menu item (Admin only)
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<MenuItemResponse>> CreateOrUpdateMenuItem([FromBody] CreateMenuItemRequest request, [FromHeader] string adminKey)
    {
        // Simple admin key authentication for prototype
        var validAdminKey = _configuration["AdminKey"] ?? "admin123";
        if (string.IsNullOrEmpty(adminKey) || adminKey != validAdminKey)
        {
            return Unauthorized("Invalid admin key");
        }

        // Get the first restaurant (assuming single restaurant for prototype)
        var restaurant = await _context.Restaurants.FirstOrDefaultAsync();
        if (restaurant == null)
        {
            return BadRequest("No restaurant found. Please seed the database first.");
        }

        var menuItem = new MenuItem
        {
            Name = request.Name,
            Description = request.Description,
            Price = request.Price,
            Category = request.Category,
            IsAvailable = request.IsAvailable,
            ImageUrl = request.ImageUrl,
            IsVegetarian = request.IsVegetarian,
            PreparationTimeMinutes = request.PreparationTimeMinutes,
            RestaurantId = restaurant.Id,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        _context.MenuItems.Add(menuItem);
        await _context.SaveChangesAsync();

        var response = new MenuItemResponse
        {
            Id = menuItem.Id,
            Name = menuItem.Name,
            Description = menuItem.Description,
            Price = menuItem.Price,
            Category = menuItem.Category,
            IsAvailable = menuItem.IsAvailable,
            ImageUrl = menuItem.ImageUrl,
            IsVegetarian = menuItem.IsVegetarian,
            PreparationTimeMinutes = menuItem.PreparationTimeMinutes
        };

        return CreatedAtAction(nameof(MenuController.GetMenuItem), new { id = menuItem.Id }, response);
    }

    /// <summary>
    /// Update an existing menu item (Admin only)
    /// </summary>
    [HttpPut("{id}")]
    public async Task<ActionResult<MenuItemResponse>> UpdateMenuItem(int id, [FromBody] UpdateMenuItemRequest request, [FromHeader] string adminKey)
    {
        // Simple admin key authentication for prototype
        var validAdminKey = _configuration["AdminKey"] ?? "admin123";
        if (string.IsNullOrEmpty(adminKey) || adminKey != validAdminKey)
        {
            return Unauthorized("Invalid admin key");
        }

        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem == null)
            return NotFound();

        menuItem.Name = request.Name;
        menuItem.Description = request.Description;
        menuItem.Price = request.Price;
        menuItem.Category = request.Category;
        menuItem.IsAvailable = request.IsAvailable;
        menuItem.ImageUrl = request.ImageUrl;
        menuItem.IsVegetarian = request.IsVegetarian;
        menuItem.PreparationTimeMinutes = request.PreparationTimeMinutes;
        menuItem.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var response = new MenuItemResponse
        {
            Id = menuItem.Id,
            Name = menuItem.Name,
            Description = menuItem.Description,
            Price = menuItem.Price,
            Category = menuItem.Category,
            IsAvailable = menuItem.IsAvailable,
            ImageUrl = menuItem.ImageUrl,
            IsVegetarian = menuItem.IsVegetarian,
            PreparationTimeMinutes = menuItem.PreparationTimeMinutes
        };

        return Ok(response);
    }

    /// <summary>
    /// Delete a menu item (Admin only)
    /// </summary>
    [HttpDelete("{id}")]
    public async Task<ActionResult> DeleteMenuItem(int id, [FromHeader] string adminKey)
    {
        // Simple admin key authentication for prototype
        var validAdminKey = _configuration["AdminKey"] ?? "admin123";
        if (string.IsNullOrEmpty(adminKey) || adminKey != validAdminKey)
        {
            return Unauthorized("Invalid admin key");
        }

        var menuItem = await _context.MenuItems.FindAsync(id);
        if (menuItem == null)
            return NotFound();

        _context.MenuItems.Remove(menuItem);
        await _context.SaveChangesAsync();

        return NoContent();
    }
}

// Response DTOs for menu endpoints
public class MenuResponse
{
    public List<CategoryResponse> Categories { get; set; } = new();
    public int TotalItems { get; set; }
}

public class CategoryResponse
{
    public string Name { get; set; } = string.Empty;
    public List<MenuItemResponse> Items { get; set; } = new();
}
