using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ConversationDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Claims;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.presentation.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class ChatController(IChatService _chatService) : ControllerBase
    {
        private string UserId => User.FindFirst(ClaimTypes.NameIdentifier)!.Value;
        [HttpPost("conversations")]
        public async Task<IActionResult> StartConversation(StartConversationDto dto)
        {
            var result = await _chatService.GetOrCreateConversationAsync(UserId, dto);
            return Ok(result);
        }

        [HttpGet("conversations")]
        public async Task<IActionResult> GetMyConversations()
        {
            var result = await _chatService.GetMyConversationsAsync(UserId);
            return Ok(result);
        }

        [HttpGet("conversations/{conversationId:int}/messages")]
        public async Task<IActionResult> GetMessages(int conversationId)
        {
            try
            {
                var result = await _chatService.GetMessagesAsync(conversationId, UserId);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("messages")]
        public async Task<IActionResult> SendMessage(SendMessageDto dto)
        {
            try
            {
                var result = await _chatService.SendMessageAsync(UserId, dto);
                return Ok(result);
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }

        [HttpPost("conversations/{conversationId:int}/read")]
        public async Task<IActionResult> MarkAsRead(int conversationId)
        {
            try
            {
                await _chatService.MarkAsReadAsync(conversationId, UserId);
                return NoContent();
            }
            catch (UnauthorizedAccessException)
            {
                return Forbid();
            }
        }
    }
}
