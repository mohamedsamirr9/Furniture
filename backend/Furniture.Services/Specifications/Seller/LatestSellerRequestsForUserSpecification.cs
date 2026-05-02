using Furniture.Domain.Models;

namespace Furniture.Services.Specifications;

/// <summary>Returns the single most recent seller request for a user (any status).</summary>
public class LatestSellerRequestsForUserSpecification : BaseSpecificationscs<SellerRequest, int>
{
    public LatestSellerRequestsForUserSpecification(string userId)
        : base(r => r.UserId == userId)
    {
        AddOrderByDescending(r => r.CreatedAt);
        AddInclude(r => r.User);
        ApplyPagination(1, 1);
    }
}
