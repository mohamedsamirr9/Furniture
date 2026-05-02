using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;

namespace Furniture.Services.Specifications;

public class PendingSellerRequestForUserSpecification : BaseSpecificationscs<SellerRequest, int>
{
    public PendingSellerRequestForUserSpecification(string userId)
        : base(r => r.UserId == userId && r.Status == SellerRequestStatus.Pending)
    {
        AddInclude(r => r.User);
    }
}
