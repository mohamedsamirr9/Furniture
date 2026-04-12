namespace Furniture.shared.Dtos.Order;

public class OrderResponseDTO
{
    public int OrderId { get; set; }
    public decimal SubTotal { get; set; }
    public decimal ShippingCost { get; set; }
    public decimal TotalPrice { get; set; }
    public string City { get; set; } 
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = "Order created successfully!";
}