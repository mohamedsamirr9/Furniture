using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Message
    {
        public int Id { get; set; }
        public int ConversationId {  get; set; }
        public Conversation Conversation { get; set; } = null!;
        public string SenderId { get; set; } = null!;
        public ApplicationUser Sender { get; set; } = null!;
        public string Content { get; set; } = null!;
        public DateTime SentAt { get; set; } = DateTime.UtcNow;
        public bool IsRead { get; set; } = false;
    }
}
