using Furniture.shared.Dtos.ProductDtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.CategoryDto
{
    public class CategoryListDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = null!;
        public string? NameAr { get; set; }
        public string Name { get; set; } = null!;
        public string DescriptionEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public string? Description { get; set; }
        public string? Image { get; set; }

     
    }
}
