using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.ComplaintsDto
{
    public class ComplaintCreateDto
    {
        public string? ImageUrl { get; set; }
        public string? Description { get; set; }
        public int OrderId { get; set; }
    }
}
