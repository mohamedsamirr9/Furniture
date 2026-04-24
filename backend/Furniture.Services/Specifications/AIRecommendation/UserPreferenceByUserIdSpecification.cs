using Furniture.Domain.Models;

namespace Furniture.Services.Specifications.AIRecommendation;

public class UserPreferenceByUserIdSpecification : BaseSpecificationscs<UserPreference, int>
{
    public UserPreferenceByUserIdSpecification(string userId)
        : base(u => u.UserId == userId)
    {
    }
}