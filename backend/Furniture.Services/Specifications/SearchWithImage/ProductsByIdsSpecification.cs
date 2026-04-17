using Furniture.Domain.Models;

namespace Furniture.Services.Specifications.SearchWithImage;

public class ProductsByIdsSpecification : BaseSpecificationscs<Product, int>
{
    public ProductsByIdsSpecification(List<int> ids)
        : base(p => ids.Contains(p.Id))
    {
        AddInclude(p => p.Images);
    }
}