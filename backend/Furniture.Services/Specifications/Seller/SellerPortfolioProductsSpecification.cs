using Furniture.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Furniture.Services.Specifications
{
    public class SellerPortfolioProductsSpecification : BaseSpecificationscs<Product, int>
    {
        public SellerPortfolioProductsSpecification(string sellerId)
            : base(p => p.SellerId == sellerId)
        {
            AddInclude(p => p.Category);
            AddInclude(p => p.Images);
            AddInclude(p => p.Reviews);
            AddInclude(query => query.Include(p => p.OrderItems).ThenInclude(oi => oi.Order));
            AddOrderByDescending(p => p.CreatedAt);
        }
    }
}
