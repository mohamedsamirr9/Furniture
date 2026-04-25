namespace Furniture.shared.Dtos.Seller;

public class SellerPayoutDTO
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public decimal CommissionAmount { get; set; }
    public string Status { get; set; } = null!;
    public DateTime CreatedAt { get; set; }
    public DateTime? PaidAt { get; set; }
    public string? TransactionId { get; set; }
}
