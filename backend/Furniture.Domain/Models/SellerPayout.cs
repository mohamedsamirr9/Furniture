using Furniture.Domain.Models.Enum;

namespace Furniture.Domain.Models
{
    public class SellerPayout
    {
        public int Id { get; set; }
        public int SellerProfileId { get; set; }
        public int OrderId { get; set; }
        public decimal OrderItemsTotal { get; set; }
        public decimal CommissionAmount { get; set; }
        public decimal NetAmount { get; set; }
        public PayoutStatus Status { get; set; }
        public DateTime? PaidAt { get; set; }
        
        public string? PaymobTransactionId { get; set; }
        
        public string? PayoutTransactionId { get; set; }
        public string? FailureReason { get; set; }
        public DateTime? ProcessedAt { get; set; }
        
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public SellerProfile SellerProfile { get; set; } = null!;
        public Order Order { get; set; } = null!;
    }
}