namespace Furniture.shared.Dtos.SellerDto;

/// <summary>Partial update: only non-null fields are applied.</summary>
public class UpdateSellerProfileDto
{
    public string? Name { get; set; }
    public string? Location { get; set; }
    public string? Bio { get; set; }
    public string? ProfileImageUrl { get; set; }

    // Bank/payment details
    public string? BankName { get; set; }
    public string? BankAccountNumber { get; set; }
    public string? BankCode { get; set; }
    public string? NationalId { get; set; }
    public string? PaymobMerchantId { get; set; }
}
