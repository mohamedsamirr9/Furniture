using Furniture.Domain.Models;
using System.Linq.Expressions;

namespace Furniture.Services.Specifications.Order
{
    public class SellerOrdersSpecification : BaseSpecificationscs<Domain.Models.Order, int>
    {
        public SellerOrdersSpecification(string sellerId) 
            : base(o => (o.OrderItems != null && o.OrderItems.Any(i => i.Product.SellerId == sellerId))
                     || (o.Offer != null && o.Offer.SellerId == sellerId))
        {
            AddInclude("OrderItems.Product.Images");
            AddInclude("Offer.CustomRequest");
            AddInclude(o => o.Payment!);
            AddOrderByDescending(o => o.OrderDate);
        }
    }
}
