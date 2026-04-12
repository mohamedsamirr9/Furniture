using System.ComponentModel.DataAnnotations;

namespace Furniture.shared.Dtos.Seller;

public class CreateSellerProfileDTO
{
    [Required]
    public string StoreName { get; set; } = null!;
        
    public string? StoreDescription { get; set; }
        
    [Required]
    public string BankName { get; set; } = null!;
        
    [Required]
    public string BankAccountNumber { get; set; } = null!;
}