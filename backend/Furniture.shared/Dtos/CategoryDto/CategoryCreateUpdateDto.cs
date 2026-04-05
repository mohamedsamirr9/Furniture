using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.CategoryDto
{
    public class CategoryCreateUpdateDto
    {
        public string Name { get; set; } = null!;
        public string? Description { get; set; }
        public string? Image { get; set; }
    }
}
