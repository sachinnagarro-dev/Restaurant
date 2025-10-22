namespace TableOrder.Backend.DTOs;

public class CreateMenuItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public string? ImageUrl { get; set; }
    public bool IsVegetarian { get; set; } = true;
    public int PreparationTimeMinutes { get; set; } = 15;
}

public class UpdateMenuItemRequest
{
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; } = true;
    public string? ImageUrl { get; set; }
    public bool IsVegetarian { get; set; } = true;
    public int PreparationTimeMinutes { get; set; } = 15;
}
