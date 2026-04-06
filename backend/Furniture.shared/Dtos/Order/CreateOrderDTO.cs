using System.ComponentModel.DataAnnotations;

namespace Furniture.shared.Dtos.Order;

public class CreateOrderDTO
{
    [Required(ErrorMessage = "Shipping address is required")]
    public string ShippingAddress { get; set; } = string.Empty;
        
    public string? Notes { get; set; }
}