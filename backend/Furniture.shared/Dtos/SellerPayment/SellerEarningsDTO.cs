namespace Furniture.shared.Dtos.Seller;

public class SellerEarningsDTO
{
    public decimal TotalSales { get; set; }
    public decimal TotalCommission { get; set; }
    public decimal NetEarnings { get; set; }
    public decimal PendingAmount { get; set; }
    public decimal PaidAmount { get; set; }
}