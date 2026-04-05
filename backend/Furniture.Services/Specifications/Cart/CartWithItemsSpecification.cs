using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class CartWithItemsSpecification : BaseSpecificationscs<Cart, int>
    {
        public CartWithItemsSpecification(string userId)
            : base(c => c.UserId == userId)
        {
            AddInclude(c => c.CartItems);
            AddInclude("CartItems.Product");     
            AddInclude("CartItems.Product.Images"); 
        }
    }
}


