using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class BlockedSellerProfilesByUserIdsSpecification : BaseSpecificationscs<SellerProfile, int>
    {
        public BlockedSellerProfilesByUserIdsSpecification(IEnumerable<string> sellerIds)
            : base(sp => sellerIds.Contains(sp.UserId) && sp.IsBlocked)
        {
        }
    }
}