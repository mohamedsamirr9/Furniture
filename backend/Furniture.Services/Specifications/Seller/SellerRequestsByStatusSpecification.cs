using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;

namespace Furniture.Services.Specifications;

public class SellerRequestsByStatusSpecification : BaseSpecificationscs<SellerRequest, int>
{
    public SellerRequestsByStatusSpecification(SellerRequestStatus status)
        : base(r => r.Status == status)
    {
        AddOrderByDescending(r => r.CreatedAt);
        AddInclude(r => r.User);
    }
}
