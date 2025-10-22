namespace TableOrder.Backend.Models;

public class Order
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Received;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? SpecialInstructions { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Table Table { get; set; } = null!;
    public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

public enum OrderStatus
{
    Received,
    Preparing,
    Ready,
    Served,
    Closed
}
