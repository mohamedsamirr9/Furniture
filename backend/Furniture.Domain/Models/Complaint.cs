using Furniture.Domain.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Complaint
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public ComplaintStatus Status { get; set; }

        //rel
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; } = null!;

      
    }
}
