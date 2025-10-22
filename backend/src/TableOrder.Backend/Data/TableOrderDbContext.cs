using Microsoft.EntityFrameworkCore;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.Data;

public class TableOrderDbContext : DbContext
{
    public TableOrderDbContext(DbContextOptions<TableOrderDbContext> options) : base(options)
    {
    }

    public DbSet<Restaurant> Restaurants { get; set; }
    public DbSet<Table> Tables { get; set; }
    public DbSet<MenuItem> MenuItems { get; set; }
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderItem> OrderItems { get; set; }
    public DbSet<Payment> Payments { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // Restaurant configuration
        modelBuilder.Entity<Restaurant>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Phone).HasMaxLength(20);
            entity.Property(e => e.Email).HasMaxLength(100);
            entity.HasIndex(e => e.Email).IsUnique();
        });

        // Table configuration
        modelBuilder.Entity<Table>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Number).IsRequired();
            entity.Property(e => e.Capacity).IsRequired();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.HasIndex(e => new { e.RestaurantId, e.Number }).IsUnique();
            
            entity.HasOne(e => e.Restaurant)
                .WithMany(r => r.Tables)
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MenuItem configuration
        modelBuilder.Entity<MenuItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Description).HasMaxLength(1000);
            entity.Property(e => e.Price).HasPrecision(10, 2);
            entity.Property(e => e.Category).IsRequired().HasMaxLength(100);
            entity.Property(e => e.ImageUrl).HasMaxLength(500);
            
            entity.HasOne(e => e.Restaurant)
                .WithMany(r => r.MenuItems)
                .HasForeignKey(e => e.RestaurantId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Order configuration
        modelBuilder.Entity<Order>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.SubTotal).HasPrecision(10, 2);
            entity.Property(e => e.TaxAmount).HasPrecision(10, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(10, 2);
            entity.Property(e => e.SpecialInstructions).HasMaxLength(1000);
            
            entity.HasOne(e => e.Table)
                .WithMany(t => t.Orders)
                .HasForeignKey(e => e.TableId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // OrderItem configuration
        modelBuilder.Entity<OrderItem>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Quantity).IsRequired();
            entity.Property(e => e.UnitPrice).HasPrecision(10, 2);
            entity.Property(e => e.SpecialInstructions).HasMaxLength(500);
            
            entity.HasOne(e => e.Order)
                .WithMany(o => o.OrderItems)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
                
            entity.HasOne(e => e.MenuItem)
                .WithMany(m => m.OrderItems)
                .HasForeignKey(e => e.MenuItemId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.Property(e => e.Amount).HasPrecision(10, 2);
            entity.Property(e => e.PaymentMethod).HasConversion<string>();
            entity.Property(e => e.Status).HasConversion<string>();
            entity.Property(e => e.TransactionId).HasMaxLength(100);
            entity.Property(e => e.ReferenceNumber).HasMaxLength(50);
            
            entity.HasOne(e => e.Order)
                .WithMany(o => o.Payments)
                .HasForeignKey(e => e.OrderId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Seed data
        SeedData(modelBuilder);
    }

    private static void SeedData(ModelBuilder modelBuilder)
    {
        // Seed Restaurant
        modelBuilder.Entity<Restaurant>().HasData(
            new Restaurant
            {
                Id = 1,
                Name = "TableOrder Restaurant",
                Address = "123 Main Street, City, State 12345",
                Phone = "(555) 123-4567",
                Email = "info@tableorder.com",
                CreatedAt = DateTime.UtcNow,
                UpdatedAt = DateTime.UtcNow
            }
        );

        // Seed Tables
        modelBuilder.Entity<Table>().HasData(
            new Table { Id = 1, Number = 1, Capacity = 2, Status = TableStatus.Available, RestaurantId = 1 },
            new Table { Id = 2, Number = 2, Capacity = 4, Status = TableStatus.Available, RestaurantId = 1 },
            new Table { Id = 3, Number = 3, Capacity = 6, Status = TableStatus.Available, RestaurantId = 1 },
            new Table { Id = 4, Number = 4, Capacity = 2, Status = TableStatus.Available, RestaurantId = 1 },
            new Table { Id = 5, Number = 5, Capacity = 8, Status = TableStatus.Available, RestaurantId = 1 },
            new Table { Id = 6, Number = 6, Capacity = 4, Status = TableStatus.Available, RestaurantId = 1 },
            new Table { Id = 7, Number = 7, Capacity = 2, Status = TableStatus.Available, RestaurantId = 1 },
            new Table { Id = 8, Number = 8, Capacity = 6, Status = TableStatus.Available, RestaurantId = 1 }
        );

        // Seed Menu Items
        modelBuilder.Entity<MenuItem>().HasData(
            new MenuItem
            {
                Id = 1,
                Name = "Margherita Pizza",
                Description = "Classic tomato sauce, fresh mozzarella, and basil",
                Price = 12.99m,
                Category = "Pizza",
                IsAvailable = true,
                PreparationTimeMinutes = 20,
                RestaurantId = 1
            },
            new MenuItem
            {
                Id = 2,
                Name = "Pepperoni Pizza",
                Description = "Tomato sauce, mozzarella, and spicy pepperoni",
                Price = 14.99m,
                Category = "Pizza",
                IsAvailable = true,
                PreparationTimeMinutes = 20,
                RestaurantId = 1
            },
            new MenuItem
            {
                Id = 3,
                Name = "Caesar Salad",
                Description = "Crisp romaine lettuce, parmesan cheese, and croutons",
                Price = 8.99m,
                Category = "Salad",
                IsAvailable = true,
                PreparationTimeMinutes = 10,
                RestaurantId = 1
            },
            new MenuItem
            {
                Id = 4,
                Name = "Chicken Wings",
                Description = "Spicy buffalo wings served with ranch dressing",
                Price = 9.99m,
                Category = "Appetizer",
                IsAvailable = true,
                PreparationTimeMinutes = 15,
                RestaurantId = 1
            },
            new MenuItem
            {
                Id = 5,
                Name = "Pasta Carbonara",
                Description = "Creamy pasta with bacon, egg, and parmesan",
                Price = 13.99m,
                Category = "Pasta",
                IsAvailable = true,
                PreparationTimeMinutes = 18,
                RestaurantId = 1
            },
            new MenuItem
            {
                Id = 6,
                Name = "Grilled Salmon",
                Description = "Fresh Atlantic salmon with seasonal vegetables",
                Price = 18.99m,
                Category = "Main Course",
                IsAvailable = true,
                PreparationTimeMinutes = 25,
                RestaurantId = 1
            },
            new MenuItem
            {
                Id = 7,
                Name = "Chocolate Cake",
                Description = "Rich chocolate cake with vanilla ice cream",
                Price = 6.99m,
                Category = "Dessert",
                IsAvailable = true,
                PreparationTimeMinutes = 5,
                RestaurantId = 1
            },
            new MenuItem
            {
                Id = 8,
                Name = "Coca Cola",
                Description = "Refreshing soft drink",
                Price = 2.99m,
                Category = "Beverage",
                IsAvailable = true,
                PreparationTimeMinutes = 2,
                RestaurantId = 1
            }
        );
    }
}
