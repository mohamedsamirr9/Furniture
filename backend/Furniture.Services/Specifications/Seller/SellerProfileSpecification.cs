using Furniture.Domain.Models;

namespace Furniture.Services.Specifications.Seller;

public class SellerProfileSpecification : BaseSpecificationscs<SellerProfile, int>
{
    public SellerProfileSpecification()
        : base(null)
    {
        AddInclude(s => s.User);
        AddOrderByDescending(s => s.CreatedAt);
    }

    public SellerProfileSpecification(bool isVerified)
        : base(s => s.IsVerified == isVerified)
    {
        AddInclude(s => s.User);
        AddOrderByDescending(s => s.CreatedAt);
    }
}