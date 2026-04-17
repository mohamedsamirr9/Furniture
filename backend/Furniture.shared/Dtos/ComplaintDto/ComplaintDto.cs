using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.ComplaintsDto
{
    public class ComplaintDto
    {
        public int Id { get; set; }
        public string? Description { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public int OrderId { get; set; }
        public string UserId { get; set; } = null!;
        public string UserName { get; set; } = null!;
        public string? SellerId { get; set; }
        public int? ProductId { get; set; }
        public string? LatestReplyMessage { get; set; }
        public string? LatestReplyBy { get; set; }
        public DateTime? LatestReplyAt { get; set; }
    }
}
