using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/notifications")]
    [Authorize]
    public class NotificationsController : ControllerBase
    {
        private readonly INotificationService _notificationService;

        public NotificationsController(INotificationService notificationService)
            => _notificationService = notificationService;

        [HttpGet]
        public async Task<ActionResult<IEnumerable<NotificationDto>>> GetMine()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            var result = await _notificationService.GetMyNotificationsAsync(userId);
            return Ok(result);
        }

        [HttpPatch("{id:int}/read")]
        public async Task<IActionResult> MarkRead(int id)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
            await _notificationService.MarkAsReadAsync(id, userId);
            return NoContent();
        }
    }
}