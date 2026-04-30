namespace Furniture.shared.Dtos.Order;

public class SplitOrdersResponseDTO
{
    public int PaymentId { get; set; }
    public List<OrderResponseDTO> Orders { get; set; } = new();
    public string Message { get; set; } = "Orders created successfully!";
}

