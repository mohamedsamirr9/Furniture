using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class SellerPayoutByOrderIdSpecification : BaseSpecificationscs<SellerPayout, int>
    {
        public SellerPayoutByOrderIdSpecification(int orderId)
            : base(p => p.OrderId == orderId)
        {
            AddInclude(p => p.Order);
        }
    }
}