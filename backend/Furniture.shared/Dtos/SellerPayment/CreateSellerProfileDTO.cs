using System.ComponentModel.DataAnnotations;

namespace Furniture.shared.Dtos.Seller;

public class CreateSellerProfileDTO
{
    public string StoreName { get; set; } = null!;
    public string? StoreDescription { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankCode { get; set; }    
    public string? NationalId { get; set; }  
}