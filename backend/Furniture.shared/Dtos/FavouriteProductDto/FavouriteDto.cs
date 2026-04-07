using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.FavouriteProductDto
{
    public class FavouriteDto
    {
        public int FavouriteId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = null!;
        public decimal ProductPrice { get; set; }
        public string CategoryName { get; set; } = null!;
        public string SellerName { get; set; } = null!;
        public string? MainImage { get; set; }
        public bool IsAvailable { get; set; }
    }
}
