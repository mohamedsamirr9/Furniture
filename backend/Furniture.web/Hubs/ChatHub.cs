using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ConversationDtos;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.SignalR;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Persistence.Repositories;
using Furniture.Domain.Models;

namespace Furniture.web.Hubs
{
    [Authorize]
    public class ChatHub : Hub
    {
        private readonly IChatService _chatService;
        private readonly IUnitOfWork _unitOfWork;

        public ChatHub(IChatService chatService, IUnitOfWork unitOfWork)
        {
            _chatService = chatService;
            _unitOfWork = unitOfWork;
        }

        public async Task JoinConversation(int conversationId)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            var groupName = $"conv_{conversationId}";
            await Groups.AddToGroupAsync(Context.ConnectionId, groupName);
            await Clients.Caller.SendAsync("JoinedConversation", conversationId);
        }

        public async Task LeaveConversation(int conversationId)
        {
            var groupName = $"conv_{conversationId}";
            await Groups.RemoveFromGroupAsync(Context.ConnectionId, groupName);
        }

        public async Task SendMessage(int conversationId, string content)
        {
            var senderId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(senderId))
            {
                throw new HubException("User not authenticated");
            }

            var dto = new SendMessageDto
            {
                ConversationId = conversationId,
                Content = content
            };

            var message = await _chatService.SendMessageAsync(senderId, dto);

            // Send to conversation group EXCLUDING the sender (sender already added optimistically)
            var groupName = $"conv_{conversationId}";
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("ReceiveMessage", message);

            // Get the conversation to find the other participant
            var conversation = await _unitOfWork.GetRepository<Conversation, int>().GetByIdAsync(conversationId);
            if (conversation != null)
            {
                var receiverId = conversation.CustomerId == senderId
                    ? conversation.SellerId
                    : conversation.CustomerId;

                // Send to receiver's personal group (if not the sender)
                if (receiverId != senderId)
                {
                    await Clients.Group($"user_{receiverId}").SendAsync("ReceiveMessage", message);
                }
            }
        }

        public async Task MarkAsRead(int conversationId)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                throw new HubException("User not authenticated");
            }

            await _chatService.MarkAsReadAsync(conversationId, userId);
            var groupName = $"conv_{conversationId}";
            await Clients.Group(groupName).SendAsync("MessagesRead", conversationId, userId);
        }

        public async Task TypingIndicator(int conversationId)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return;
            }

            var groupName = $"conv_{conversationId}";
            await Clients.GroupExcept(groupName, Context.ConnectionId).SendAsync("UserTyping", conversationId, userId);
        }

        public override async Task OnConnectedAsync()
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.AddToGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnConnectedAsync();
        }

        public override async Task OnDisconnectedAsync(Exception? exception)
        {
            var userId = Context.User?.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (!string.IsNullOrEmpty(userId))
            {
                await Groups.RemoveFromGroupAsync(Context.ConnectionId, $"user_{userId}");
            }
            await base.OnDisconnectedAsync(exception);
        }
    }
}
