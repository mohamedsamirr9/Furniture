using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.ProductDtos
{
    public class ProductDetailsDto
    {
        public int Id { get; set; }
        public string NameEn { get; set; } = null!;
        public string? NameAr { get; set; }
        public string Name { get; set; } = null!;
        public string DescriptionEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public string? Description { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        public bool IsCustomized { get; set; }
        public DateTime CreatedAt { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = string.Empty;
        public string SellerName { get; set; } = string.Empty;

        public List<string> Images { get; set; } = new();
        public ICollection<ReviewDto> Reviews { get; set; } = new List<ReviewDto>();
    }
}
