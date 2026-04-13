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
        public DateTime CreatedAt { get; set; }
        public string Status { get; set; }
        public int OrderId { get; set; }
        
    }
}
