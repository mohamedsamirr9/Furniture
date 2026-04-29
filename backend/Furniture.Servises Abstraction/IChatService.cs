using Furniture.shared.Dtos.ConversationDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Servises_Abstraction
{
    public interface IChatService
    {
        Task<ConversationDto> GetOrCreateConversationAsync(string currentUserId, StartConversationDto dto);
        Task<IEnumerable<ConversationDto>> GetMyConversationsAsync(string userId);
        Task<IEnumerable<MessageDto>> GetMessagesAsync(int conversationId, string userId);
        Task<MessageDto> SendMessageAsync(string senderId, SendMessageDto dto);
        Task MarkAsReadAsync(int conversationId, string userId);

    }
}
