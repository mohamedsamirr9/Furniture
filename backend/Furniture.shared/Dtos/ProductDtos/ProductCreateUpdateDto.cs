using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.ProductDtos
{
    public class ProductCreateUpdateDto
    {
        public string NameEn { get; set; } = null!;
        public string? NameAr { get; set; }
        public string DescriptionEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsCustomized { get; set; }

        public int CategoryId { get; set; }
        public string SellerId { get; set; } 
        
        public List<string>? ImageUrls { get; set; } = new List<string>(); 
    }
}
