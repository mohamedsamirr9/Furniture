using System.ComponentModel.DataAnnotations;

namespace Furniture.shared.Dtos.Order;

public class UpdateOrderStatusDTO
{
    [Required]
    public string Status { get; set; } = string.Empty;
        
    public string? DeclineReason { get; set; }
}