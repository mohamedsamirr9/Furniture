using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class SellerPayoutSpecification : BaseSpecificationscs<SellerPayout, int>
    {
        public SellerPayoutSpecification(int sellerProfileId)
            : base(p => p.SellerProfileId == sellerProfileId)
        {
            AddInclude(p => p.Order);
            AddInclude("Order.Payment");
            AddOrderByDescending(p => p.CreatedAt);
        }
    }
}