using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications.Chat
{
    public class ConversationsByUserSpec : BaseSpecificationscs<Conversation, int>
    {
        public ConversationsByUserSpec(string userId)
            : base(c => c.SellerId == userId || c.CustomerId == userId)
        {
            AddInclude(c => c.Seller);
            AddInclude(c => c.Customer);
            AddInclude(c=>c.Messages);
            AddOrderByDescending(c => c.CreatedAt);
        }
    }
}
