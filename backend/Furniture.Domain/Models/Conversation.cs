using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Conversation
    {
        public int Id {  get; set; }
        public string CustomerId { get; set; } = null!;
        public ApplicationUser Customer { get; set; } = null!;
        public string SellerId { get; set; } = null!;
        public ApplicationUser Seller { get; set; } = null!;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public ICollection<Message> Messages { get; set; } = new List<Message>();



    }
}
