namespace Furniture.shared.Dtos.Order;

public class OrderDTO
{
    public int Id { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalPrice { get; set; }
    public string City { get; set; } 
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ShippingAddress { get; set; }
    public DateTime CreatedAt { get; set; }
        
    public string? UserName { get; set; }  
    public bool IsCustom { get; set; }
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public List<OrderItemDTO> OrderItems { get; set; } = new();
}