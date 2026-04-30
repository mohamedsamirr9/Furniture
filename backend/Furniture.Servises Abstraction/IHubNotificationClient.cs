using Furniture.shared.Dtos;

namespace Furniture.Servises_Abstraction
{
    public interface IHubNotificationClient
    {
        
        Task SendNotificationToUserAsync(string userId, NotificationDto notification);
        Task BroadcastNotificationAsync(IEnumerable<string> userIds, NotificationDto notification);
    }
}