namespace TableOrder.Backend.DTOs;

public class MenuItemResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public decimal Price { get; set; }
    public string Category { get; set; } = string.Empty;
    public bool IsAvailable { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsVegetarian { get; set; }
    public int PreparationTimeMinutes { get; set; }
}

public class TableResponse
{
    public int Id { get; set; }
    public int Number { get; set; }
    public int Capacity { get; set; }
    public string Status { get; set; } = string.Empty;
}
