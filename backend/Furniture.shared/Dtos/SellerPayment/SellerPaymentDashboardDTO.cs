namespace Furniture.shared.Dtos.Seller;

public class SellerPaymentDashboardDTO
{
    public OnlineEarningsDTO OnlineEarnings { get; set; } = new();
    public CashSummaryDTO CashSummary { get; set; } = new();
    public List<RecentPayoutDTO> RecentPayouts { get; set; } = new();
}

public class OnlineEarningsDTO
{
    public decimal TotalEarnings { get; set; }
    public decimal PendingPayout { get; set; }
    public decimal TotalPaid { get; set; }
}

public class CashSummaryDTO
{
    public int TotalCashOrders { get; set; }
    public decimal CashAmount { get; set; }
    public decimal PendingCommission { get; set; }
    public decimal MaxLimit { get; set; }
    public decimal RemainingLimit { get; set; }
}

public class RecentPayoutDTO
{
    public int OrderId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = "Pending";
    public DateTime Date { get; set; }
}
