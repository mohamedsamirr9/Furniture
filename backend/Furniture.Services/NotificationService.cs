using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Domain.Models.Enum;
using Furniture.Domain.Specifications.NotificationSpecifications;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;

namespace Furniture.Services
{
    public class NotificationService : INotificationService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly IHubNotificationClient _hubNotificationClient;

        public NotificationService(
            IUnitOfWork unitOfWork,
            UserManager<ApplicationUser> userManager,
            IHubNotificationClient hubNotificationClient)
        {
            _unitOfWork = unitOfWork;
            _userManager = userManager;
            _hubNotificationClient = hubNotificationClient;
        }

        public async Task NotifyAllSellersAsync(string title, string message, int? customRequestId = null)
        {
            var sellers = await _userManager.Users
                .Where(u => u.Role == Roles.seller)
                .Select(u => u.Id)
                .ToListAsync();

            if (!sellers.Any()) return;

            var repo = _unitOfWork.GetRepository<Notification, int>();
            var now = DateTime.UtcNow;

            var notifications = sellers.Select(sellerId => new Notification
            {
                UserId = sellerId,
                Title = title,
                Message = message,
                CustomRequestId = customRequestId,
                IsRead = false,
                CreatedAt = now
            }).ToList();

            foreach (var notif in notifications)
                await repo.AddAsync(notif);

            await _unitOfWork.SaveChangesAsync();

            foreach (var notif in notifications)
            {
                var notifDto = new NotificationDto
                {
                    Id = notif.Id,
                    Title = notif.Title,
                    Message = notif.Message,
                    CustomRequestId = notif.CustomRequestId,
                    CreatedAt = notif.CreatedAt,
                    IsRead = notif.IsRead
                };

                await _hubNotificationClient.SendNotificationToUserAsync(notif.UserId, notifDto);
            }
        }

        public async Task<IEnumerable<NotificationDto>> GetMyNotificationsAsync(string userId)
        {
            var repo = _unitOfWork.GetRepository<Notification, int>();
            var spec = new NotificationByUserIdSpec(userId);
            var notifications = await repo.GetAllAsync(spec);

            return notifications.Select(n => new NotificationDto
            {
                Id = n.Id,
                Title = n.Title,
                Message = n.Message,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                CustomRequestId = n.CustomRequestId
            });
        }

        public async Task MarkAsReadAsync(int notificationId, string userId)
        {
            var repo = _unitOfWork.GetRepository<Notification, int>();
            var notif = await repo.GetByIdAsync(notificationId);

            if (notif is null || notif.UserId != userId)
                throw new Exception("Not found or unauthorized");

            notif.IsRead = true;
            repo.Update(notif);
            await _unitOfWork.SaveChangesAsync();
        }
    }
}