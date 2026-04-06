namespace Furniture.shared.Dtos.Order;

public class OrderResponseDTO
{
    public int OrderId { get; set; }
    public decimal TotalPrice { get; set; }
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = string.Empty;
    public string Message { get; set; } = "Order created successfully!";
}