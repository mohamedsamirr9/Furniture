using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Furniture.web.Hubs;
using Microsoft.AspNetCore.SignalR;

namespace Furniture.web.Hubs
{
    
    public class HubNotificationClient : IHubNotificationClient
    {
        private readonly IHubContext<NotificationHub> _hubContext;

        public HubNotificationClient(IHubContext<NotificationHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task SendNotificationToUserAsync(string userId, NotificationDto notification)
        {
            await _hubContext.Clients
                .Group($"user_{userId}")
                .SendAsync("ReceiveNotification", notification);
        }

        public async Task BroadcastNotificationAsync(IEnumerable<string> userIds, NotificationDto notification)
        {
            foreach (var userId in userIds)
            {
                await SendNotificationToUserAsync(userId, notification);
            }
        }
    }
}