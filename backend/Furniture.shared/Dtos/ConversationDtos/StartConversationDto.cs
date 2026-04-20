using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.ConversationDtos
{
    public class StartConversationDto
    {
        public string OtherUserId { get; set; } = null!;
        public string FirstMessage { get; set; } = null!;
    }
}
