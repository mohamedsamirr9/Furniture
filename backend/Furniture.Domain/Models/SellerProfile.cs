namespace Furniture.Domain.Models

{
    public class SellerProfile
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string StoreName { get; set; } = null!;
        public string? StoreDescription { get; set; }
        public string? PaymobMerchantId { get; set; }
        public string? BankAccountNumber { get; set; }
        public string? BankName { get; set; }
        public decimal CommissionRate { get; set; } = 10m;
        public bool IsVerified { get; set; } = false;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        
        public ApplicationUser User { get; set; } = null!;
        public ICollection<SellerPayout> Payouts { get; set; } = new List<SellerPayout>();

    }
}