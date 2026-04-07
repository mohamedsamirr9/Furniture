using Furniture.Domain.Models;
using Furniture.shared.Dtos.ProductDtos;

namespace Furniture.Services.Specifications
{
    public class ProductCountSpecification : BaseSpecificationscs<Product, int>
    {
        public ProductCountSpecification(ProductQueryParams queryParams)
            : base(ProductFilters.BuildFilter(queryParams))
        {
        }
    }
}
