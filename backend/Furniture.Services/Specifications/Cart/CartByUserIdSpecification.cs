using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class CartByUserIdSpecification : BaseSpecificationscs<Cart, int>
    {
        public CartByUserIdSpecification(string userId)
            : base(c => c.UserId == userId)
        {
            
        }
    }
}