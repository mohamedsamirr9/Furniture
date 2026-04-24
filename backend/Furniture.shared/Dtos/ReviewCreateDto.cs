using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos
{
    public class ReviewCreateDto
    {
        public int Rating { get; set; }
        public string? Message { get; set; }
        public string? UserId { get; set; }
        public int ProductId { get; set; }
    }
}