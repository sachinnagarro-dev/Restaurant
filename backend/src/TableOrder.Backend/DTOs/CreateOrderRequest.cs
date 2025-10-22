namespace TableOrder.Backend.DTOs;

public class CreateOrderRequest
{
    public int TableId { get; set; }
    public List<OrderItemRequest> OrderItems { get; set; } = new();
    public string? SpecialInstructions { get; set; }
}

public class OrderItemRequest
{
    public int MenuItemId { get; set; }
    public int Quantity { get; set; }
    public string? SpecialInstructions { get; set; }
}
