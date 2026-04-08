using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.ProductDtos
{
    public class ProductListDto
    {      
        public int Id { get; set; }
        public string Name { get; set; } = null!;
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public int CategoryId { get; set; }
        public string CategoryName { get; set; } = null!;
        public string SellerName { get; set; } = null!;

        public string? MainImage { get; set; }
    }
}
