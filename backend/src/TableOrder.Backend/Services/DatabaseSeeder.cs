using TableOrder.Backend.Data;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.Services;

public interface IDatabaseSeeder
{
    Task SeedAsync();
}

public class DatabaseSeeder : IDatabaseSeeder
{
    private readonly TableOrderDbContext _context;
    private readonly ILogger<DatabaseSeeder> _logger;

    public DatabaseSeeder(TableOrderDbContext context, ILogger<DatabaseSeeder> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task SeedAsync()
    {
        try
        {
            _logger.LogInformation("Starting database seeding...");

            // Check if data already exists
            if (_context.Restaurants.Any())
            {
                _logger.LogInformation("Database already seeded. Skipping...");
                return;
            }

            // Seed Restaurant
            var restaurant = new Restaurant
            {
                Name = "TableOrder Restaurant",
                Address = "123 Main Street, City, State 12345",
                Phone = "(555) 123-4567",
                Email = "info@tableorder.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            };

            _context.Restaurants.Add(restaurant);
            await _context.SaveChangesAsync();

            // Seed Tables
            var tables = new List<Table>
            {
                new Table { Number = 1, Capacity = 2, Status = TableStatus.Available, RestaurantId = restaurant.Id },
                new Table { Number = 2, Capacity = 4, Status = TableStatus.Available, RestaurantId = restaurant.Id },
                new Table { Number = 3, Capacity = 6, Status = TableStatus.Available, RestaurantId = restaurant.Id },
                new Table { Number = 4, Capacity = 2, Status = TableStatus.Available, RestaurantId = restaurant.Id },
                new Table { Number = 5, Capacity = 8, Status = TableStatus.Available, RestaurantId = restaurant.Id },
                new Table { Number = 6, Capacity = 4, Status = TableStatus.Available, RestaurantId = restaurant.Id },
                new Table { Number = 7, Capacity = 2, Status = TableStatus.Available, RestaurantId = restaurant.Id },
                new Table { Number = 8, Capacity = 6, Status = TableStatus.Available, RestaurantId = restaurant.Id },
                new Table { Number = 9, Capacity = 4, Status = TableStatus.Available, RestaurantId = restaurant.Id },
                new Table { Number = 10, Capacity = 8, Status = TableStatus.Available, RestaurantId = restaurant.Id }
            };

            _context.Tables.AddRange(tables);

            // Seed Menu Items with veg/nonveg flags and images
            var menuItems = new List<MenuItem>
            {
                // Pizza Category
                new MenuItem
                {
                    Name = "Margherita Pizza",
                    Description = "Classic tomato sauce, fresh mozzarella, and basil",
                    Price = 12.99m,
                    Category = "Pizza",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1604382354936-07c5d9983bd3?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 20,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Pepperoni Pizza",
                    Description = "Tomato sauce, mozzarella, and spicy pepperoni",
                    Price = 14.99m,
                    Category = "Pizza",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1628840042765-356cda07504e?w=400",
                    IsVegetarian = false,
                    PreparationTimeMinutes = 20,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "BBQ Chicken Pizza",
                    Description = "BBQ sauce, grilled chicken, red onions, and mozzarella",
                    Price = 16.99m,
                    Category = "Pizza",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1571997478779-2adcbbe9ab2f?w=400",
                    IsVegetarian = false,
                    PreparationTimeMinutes = 25,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Veggie Supreme Pizza",
                    Description = "Tomato sauce, bell peppers, mushrooms, onions, olives, and mozzarella",
                    Price = 15.99m,
                    Category = "Pizza",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1574071318508-1cdbab80d002?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 22,
                    RestaurantId = restaurant.Id
                },

                // Pasta Category
                new MenuItem
                {
                    Name = "Pasta Carbonara",
                    Description = "Creamy pasta with bacon, egg, and parmesan",
                    Price = 13.99m,
                    Category = "Pasta",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1621996346565-e3dbc353d2e5?w=400",
                    IsVegetarian = false,
                    PreparationTimeMinutes = 18,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Spaghetti Marinara",
                    Description = "Classic tomato sauce with herbs and garlic",
                    Price = 11.99m,
                    Category = "Pasta",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1551183053-bf91a1d81141?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 15,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Chicken Alfredo",
                    Description = "Creamy alfredo sauce with grilled chicken breast",
                    Price = 15.99m,
                    Category = "Pasta",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1571997478779-2adcbbe9ab2f?w=400",
                    IsVegetarian = false,
                    PreparationTimeMinutes = 20,
                    RestaurantId = restaurant.Id
                },

                // Main Course Category
                new MenuItem
                {
                    Name = "Grilled Salmon",
                    Description = "Fresh Atlantic salmon with seasonal vegetables",
                    Price = 18.99m,
                    Category = "Main Course",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1467003909585-2f8a72700288?w=400",
                    IsVegetarian = false,
                    PreparationTimeMinutes = 25,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Grilled Chicken Breast",
                    Description = "Herb-marinated chicken breast with roasted potatoes",
                    Price = 16.99m,
                    Category = "Main Course",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1532550907401-a500c9a57435?w=400",
                    IsVegetarian = false,
                    PreparationTimeMinutes = 22,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Veggie Burger",
                    Description = "Plant-based patty with lettuce, tomato, and special sauce",
                    Price = 12.99m,
                    Category = "Main Course",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 15,
                    RestaurantId = restaurant.Id
                },

                // Salad Category
                new MenuItem
                {
                    Name = "Caesar Salad",
                    Description = "Crisp romaine lettuce, parmesan cheese, and croutons",
                    Price = 8.99m,
                    Category = "Salad",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1546793665-c74683f339c1?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 10,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Grilled Chicken Salad",
                    Description = "Mixed greens with grilled chicken, cherry tomatoes, and balsamic dressing",
                    Price = 11.99m,
                    Category = "Salad",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1512621776951-a57141f2eefd?w=400",
                    IsVegetarian = false,
                    PreparationTimeMinutes = 12,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Greek Salad",
                    Description = "Fresh tomatoes, cucumbers, olives, feta cheese, and olive oil",
                    Price = 9.99m,
                    Category = "Salad",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1540420773420-3366772f4999?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 8,
                    RestaurantId = restaurant.Id
                },

                // Appetizer Category
                new MenuItem
                {
                    Name = "Chicken Wings",
                    Description = "Spicy buffalo wings served with ranch dressing",
                    Price = 9.99m,
                    Category = "Appetizer",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1567620832904-9fe5cf23db13?w=400",
                    IsVegetarian = false,
                    PreparationTimeMinutes = 15,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Mozzarella Sticks",
                    Description = "Crispy breaded mozzarella sticks with marinara sauce",
                    Price = 7.99m,
                    Category = "Appetizer",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1565299624946-b28f40a0ca4b?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 12,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Loaded Nachos",
                    Description = "Tortilla chips with cheese, jalapeños, and sour cream",
                    Price = 8.99m,
                    Category = "Appetizer",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1571091718767-18b5b1457add?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 10,
                    RestaurantId = restaurant.Id
                },

                // Dessert Category
                new MenuItem
                {
                    Name = "Chocolate Cake",
                    Description = "Rich chocolate cake with vanilla ice cream",
                    Price = 6.99m,
                    Category = "Dessert",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1578985545062-69928b1d9587?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 5,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Tiramisu",
                    Description = "Classic Italian dessert with coffee and mascarpone",
                    Price = 7.99m,
                    Category = "Dessert",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1571877227200-a0d98ea607e9?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 5,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Cheesecake",
                    Description = "New York style cheesecake with berry compote",
                    Price = 6.99m,
                    Category = "Dessert",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1533134242443-d4fd215305ad?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 5,
                    RestaurantId = restaurant.Id
                },

                // Beverage Category
                new MenuItem
                {
                    Name = "Coca Cola",
                    Description = "Refreshing soft drink",
                    Price = 2.99m,
                    Category = "Beverage",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1581636625402-29b2a704ef13?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 2,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Fresh Orange Juice",
                    Description = "Freshly squeezed orange juice",
                    Price = 3.99m,
                    Category = "Beverage",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1621506289937-a8e4df240d0b?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 3,
                    RestaurantId = restaurant.Id
                },
                new MenuItem
                {
                    Name = "Iced Coffee",
                    Description = "Cold brew coffee with milk and ice",
                    Price = 4.99m,
                    Category = "Beverage",
                    IsAvailable = true,
                    ImageUrl = "https://images.unsplash.com/photo-1461023058943-07fcbe16d735?w=400",
                    IsVegetarian = true,
                    PreparationTimeMinutes = 5,
                    RestaurantId = restaurant.Id
                }
            };

            _context.MenuItems.AddRange(menuItems);
            await _context.SaveChangesAsync();

            _logger.LogInformation($"Database seeded successfully with {restaurant.Name}, {tables.Count} tables, and {menuItems.Count} menu items.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error occurred while seeding the database.");
            throw;
        }
    }
}
