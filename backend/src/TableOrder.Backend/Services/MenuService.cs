using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Data;
using TableOrder.Backend.DTOs;

namespace TableOrder.Backend.Services;

public interface IMenuService
{
    Task<IEnumerable<MenuItemResponse>> GetMenuItemsAsync();
    Task<IEnumerable<MenuItemResponse>> GetMenuItemsByCategoryAsync(string category);
    Task<MenuItemResponse?> GetMenuItemByIdAsync(int id);
    Task<IEnumerable<string>> GetCategoriesAsync();
}

public class MenuService : IMenuService
{
    private readonly TableOrderDbContext _context;

    public MenuService(TableOrderDbContext context)
    {
        _context = context;
    }

    public async Task<IEnumerable<MenuItemResponse>> GetMenuItemsAsync()
    {
        var menuItems = await _context.MenuItems
            .Where(m => m.IsAvailable)
            .OrderBy(m => m.Category)
            .ThenBy(m => m.Name)
            .ToListAsync();

        return menuItems.Select(MapToMenuItemResponse);
    }

    public async Task<IEnumerable<MenuItemResponse>> GetMenuItemsByCategoryAsync(string category)
    {
        var menuItems = await _context.MenuItems
            .Where(m => m.IsAvailable && m.Category == category)
            .OrderBy(m => m.Name)
            .ToListAsync();

        return menuItems.Select(MapToMenuItemResponse);
    }

    public async Task<MenuItemResponse?> GetMenuItemByIdAsync(int id)
    {
        var menuItem = await _context.MenuItems
            .FirstOrDefaultAsync(m => m.Id == id && m.IsAvailable);

        return menuItem == null ? null : MapToMenuItemResponse(menuItem);
    }

    public async Task<IEnumerable<string>> GetCategoriesAsync()
    {
        return await _context.MenuItems
            .Where(m => m.IsAvailable)
            .Select(m => m.Category)
            .Distinct()
            .OrderBy(c => c)
            .ToListAsync();
    }

    private static MenuItemResponse MapToMenuItemResponse(Models.MenuItem menuItem)
    {
        return new MenuItemResponse
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
    }
}
