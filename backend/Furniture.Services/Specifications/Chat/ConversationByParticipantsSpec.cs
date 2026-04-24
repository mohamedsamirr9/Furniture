using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications.Chat
{
    public class ConversationByParticipantsSpec : BaseSpecificationscs<Conversation, int>
    {
        public ConversationByParticipantsSpec(string userOneId, string userTwoId)
            : base(c =>
                (c.SellerId == userOneId && c.CustomerId == userTwoId) ||
                (c.SellerId == userTwoId && c.CustomerId == userOneId))
        {
            AddInclude(c => c.Seller);
            AddInclude(c => c.Customer);
            AddInclude(c => c.Messages);
        }
    }
}
