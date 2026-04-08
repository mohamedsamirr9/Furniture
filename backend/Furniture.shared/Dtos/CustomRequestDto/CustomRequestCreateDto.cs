using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.CustomRequestDto
{
    public class CustomRequestCreateDto
    {
        public string Description { get; set; } = null!;
        public decimal Budget { get; set; }
        public string? ImageUrl { get; set; }

    }
}
