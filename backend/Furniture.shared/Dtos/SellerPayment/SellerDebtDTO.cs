namespace Furniture.shared.Dtos.Seller;

public class SellerDebtDTO
{
    public decimal PendingCommission { get; set; }
    public decimal MaxAllowedCommission { get; set; }
    public bool IsBlocked { get; set; }
    public string? BlockReason { get; set; }
    public decimal RemainingBeforeBlock { get; set; }
}