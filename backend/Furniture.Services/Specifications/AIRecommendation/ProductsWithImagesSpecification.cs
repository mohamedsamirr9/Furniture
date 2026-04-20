using Furniture.Domain.Models;

namespace Furniture.Services.Specifications.AIRecommendation;

public class ProductsWithImagesSpecification : BaseSpecificationscs<Product, int>
{
    public ProductsWithImagesSpecification()
        : base(null)
    {
        AddInclude(p => p.Images);
    }
}