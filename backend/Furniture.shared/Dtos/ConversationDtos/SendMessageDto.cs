using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.ConversationDtos
{
    public class SendMessageDto
    {
        public int ConversationId { get; set; }
        public string Content { get; set; } = null!;
    }
}
