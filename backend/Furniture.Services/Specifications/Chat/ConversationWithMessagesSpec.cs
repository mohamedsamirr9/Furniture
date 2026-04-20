using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications.Chat
{
    public class ConversationWithMessagesSpec : BaseSpecificationscs<Conversation, int>
    {
        public ConversationWithMessagesSpec(int conversationId)
            : base(c => c.Id == conversationId)
        {
            AddInclude(c => c.Seller);
            AddInclude(c => c.Customer);
            AddInclude("Messages.Sender");
        }
    }
}
