using Furniture.Domain.Models;
using Furniture.Services.Specifications;

namespace Furniture.Domain.Specifications.NotificationSpecifications
{
    public class NotificationByUserIdSpec : BaseSpecificationscs<Notification, int>
    {
        public NotificationByUserIdSpec(string userId)
            : base(n => n.UserId == userId)
        {
            AddOrderByDescending(n => n.CreatedAt);
        }
    }
}