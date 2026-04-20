using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class SellerProfileByUserIdSpecification : BaseSpecificationscs<SellerProfile, int>
    {
        public SellerProfileByUserIdSpecification(string userId)
            : base(s => s.UserId == userId)
        {
            AddInclude(s => s.User);
        }
    }
}
