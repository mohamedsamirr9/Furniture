using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.ConversationDtos
{
    public class ConversationDto
    {
        public int Id { get; set; }
        public string SellerId { get; set; } = null!;
        public string SellerName { get; set; } = null!;
        public string CustomerId { get; set; } = null!;
        public string CustomerName { get; set; } = null!;
        public DateTime CreatedAt { get; set; }
        public MessageDto? LastMessage { get; set; } 
        public int UnreadCount { get; set; }

    }
}
