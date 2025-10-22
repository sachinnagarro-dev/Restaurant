using System.ComponentModel.DataAnnotations;
using TableOrder.Backend.Models;

namespace TableOrder.Backend.DTOs;

public class CreateOrderDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "TableId must be a positive number")]
    public int TableId { get; set; }

    [Required]
    [MinLength(1, ErrorMessage = "At least one item is required")]
    public List<OrderItemDto> Items { get; set; } = new();

    [MaxLength(500, ErrorMessage = "Remarks cannot exceed 500 characters")]
    public string? Remarks { get; set; }
}

public class OrderItemDto
{
    [Required]
    [Range(1, int.MaxValue, ErrorMessage = "MenuItemId must be a positive number")]
    public int MenuItemId { get; set; }

    [Required]
    [Range(1, 10, ErrorMessage = "Quantity must be between 1 and 10")]
    public int Quantity { get; set; }

    [MaxLength(200, ErrorMessage = "Special instructions cannot exceed 200 characters")]
    public string? SpecialInstructions { get; set; }
}

public class UpdateOrderStatusDto
{
    [Required]
    public OrderStatus Status { get; set; }
}

public class OrderResponseDto
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int TableNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal SubTotal { get; set; }
    public decimal TaxAmount { get; set; }
    public decimal TotalAmount { get; set; }
    public string? Remarks { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
    public List<OrderItemResponseDto> Items { get; set; } = new();
}

public class OrderItemResponseDto
{
    public int Id { get; set; }
    public int MenuItemId { get; set; }
    public string MenuItemName { get; set; } = string.Empty;
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal TotalPrice { get; set; }
    public string? SpecialInstructions { get; set; }
}

public class OrderSummaryDto
{
    public int Id { get; set; }
    public int TableId { get; set; }
    public int TableNumber { get; set; }
    public string Status { get; set; } = string.Empty;
    public decimal TotalAmount { get; set; }
    public int ItemCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
