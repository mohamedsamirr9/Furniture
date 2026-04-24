namespace Furniture.shared.Dtos.Seller;

public class SellerProfileDTO
{
    public int Id { get; set; }
    public string StoreName { get; set; } = null!;
    public string? StoreDescription { get; set; }
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankCode { get; set; }     
    public decimal CommissionRate { get; set; }
    public bool IsVerified { get; set; }
    public DateTime CreatedAt { get; set; }
}