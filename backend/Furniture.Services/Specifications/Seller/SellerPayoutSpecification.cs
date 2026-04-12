using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class SellerPayoutSpecification : BaseSpecificationscs<SellerPayout, int>
    {
        // جلب Payouts لـ Seller معين
        public SellerPayoutSpecification(int sellerProfileId)
            : base(p => p.SellerProfileId == sellerProfileId)
        {
            AddInclude(p => p.Order);
            AddOrderByDescending(p => p.CreatedAt);
        }
    }
}