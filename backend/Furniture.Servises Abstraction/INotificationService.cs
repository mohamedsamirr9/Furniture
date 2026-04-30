using Furniture.shared.Dtos;

namespace Furniture.Servises_Abstraction
{
    public interface INotificationService
    {
        Task NotifyAllSellersAsync(string title, string message, int? customRequestId = null);
        Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(string userId);
        Task MarkAsReadAsync(int notificationId, string userId);
    }
}