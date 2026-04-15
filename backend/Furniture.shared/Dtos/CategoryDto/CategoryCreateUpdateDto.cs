using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.CategoryDto
{
    public class CategoryCreateUpdateDto
    {
        public string NameEn { get; set; } = null!;
        public string? NameAr { get; set; }
        public string DescriptionEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public string? Image { get; set; }
    }
}
