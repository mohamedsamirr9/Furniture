using AutoMapper;
using Furniture.Domain.InterfacesRepositories;
using Furniture.Domain.Models;
using Furniture.Services.Specifications.Chat;
using Furniture.Servises_Abstraction;
using Furniture.shared.Dtos.ConversationDtos;
using Microsoft.VisualBasic;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services
{
    public class ChatService : IChatService
    {
        private readonly IUnitOfWork _unitOfWork;
        private readonly IMapper _mapper;

        public ChatService(IUnitOfWork unitOfWork, IMapper mapper)
        {
            _unitOfWork = unitOfWork;
            _mapper = mapper;
        }

        public async Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, string userId)
        {
            var spec = new ConversationWithMessagesSpec(conversationId);
            var conversation = await _unitOfWork.GetRepository<Conversation, int>().GetByIdAsync(spec);

            if (conversation == null)
                throw new Exception("Conversation not found");
            if (conversation.CustomerId != userId &&
                conversation.SellerId != userId)
                throw new UnauthorizedAccessException("Not your conversation");
            return _mapper.Map<IEnumerable<MessageDto>>(
                conversation.Messages.OrderBy(m => m.SentAt));
        }

        public async Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(string userId)
        {
            var spec = new ConversationsByUserSpec(userId);
            var conversations = await _unitOfWork.GetRepository<Conversation, int>().GetAllAsync(spec);
            return conversations.Select(c => MapConversation(c, userId));


        }

        public async Task<ConversationDto> GetOrCreateConversationAsync(string currentUserId, StartConversationDto dto)
        {
            var repo = _unitOfWork.GetRepository<Conversation, int>();
            var spec = new ConversationByParticipantsSpec(currentUserId, dto.OtherUserId);
            var existing =  await repo.GetByIdAsync(spec);
            if (existing != null)
            {
                await SendMessageAsync(currentUserId, new SendMessageDto
                {
                    ConversationId = existing.Id,
                    Content = dto.FirstMessage,
                });

                var updatedSpec = new ConversationWithMessagesSpec(existing.Id);
                var updated = await repo.GetByIdAsync(updatedSpec);
                return MapConversation(updated!, currentUserId);
            }

            var conversation = new Conversation
            {
                CustomerId = currentUserId,
                SellerId = dto.OtherUserId,
            };
            await repo.AddAsync(conversation);
            await _unitOfWork.SaveChangesAsync();

            await SendMessageAsync(currentUserId, new SendMessageDto
            {
                ConversationId = conversation.Id,
                Content = dto.FirstMessage,
            });

            var freshSpec = new ConversationWithMessagesSpec(conversation.Id);
            var fresh = await repo.GetByIdAsync(freshSpec);
            return MapConversation(fresh!, currentUserId);

        }

        public async Task MarkAsReadAsync(int conversationId, string userId)
        {
            var spec = new ConversationWithMessagesSpec(conversationId);
            var conversation = await _unitOfWork.GetRepository<Conversation,int>().GetByIdAsync(spec);


            if (conversation == null) return;

            if (conversation.CustomerId != userId &&
                conversation.SellerId != userId)
                throw new UnauthorizedAccessException("Not your conversation");

            var unreadMessages = conversation.Messages
                .Where(m => m.SenderId != userId && !m.IsRead)
                .ToList();

            foreach (var msg in unreadMessages)
            {
                msg.IsRead = true;
                _unitOfWork.GetRepository<Message, int>().Update(msg);
            }

            await _unitOfWork.SaveChangesAsync();
        }

        public async Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto)
        {
            var conversation = await _unitOfWork.GetRepository<Conversation, int>().GetByIdAsync(dto.ConversationId);
            if (conversation == null)
                throw new Exception("Conversation not found");
            if (conversation.CustomerId != senderId &&
                conversation.SellerId != senderId)
                throw new UnauthorizedAccessException("Not your conversation");
            var message = new Message
            {
                ConversationId = dto.ConversationId,
                SenderId = senderId,
                Content = dto.Content,
            };
            await _unitOfWork.GetRepository<Message, int>().AddAsync(message);
            await _unitOfWork.SaveChangesAsync();

            return _mapper.Map<MessageDto>(message);
        }
        private ConversationDto MapConversation(Conversation c, string currentUserId)
        {
            var lastMessage = c.Messages.OrderByDescending(m => m.SentAt).FirstOrDefault();
            var unreadCount = c.Messages.Count(m => m.SenderId != currentUserId && !m.IsRead);

            return new ConversationDto
            {
                Id = c.Id,
                CustomerId = c.CustomerId,
                CustomerName = c.Customer?.Name ?? string.Empty,
                SellerId = c.SellerId,
                SellerName = c.Seller?.Name ?? string.Empty,
                CreatedAt = c.CreatedAt,
                UnreadCount = unreadCount,
                LastMessage = lastMessage == null ? null : new MessageDto
                {
                    Id = lastMessage.Id,
                    ConversationId = lastMessage.ConversationId,
                    SenderId = lastMessage.SenderId,
                    SenderName = lastMessage.Sender?.Name ?? string.Empty,
                    Content = lastMessage.Content,
                    SentAt = lastMessage.SentAt,
                    IsRead = lastMessage.IsRead
                }
            };
        }
    }
}
