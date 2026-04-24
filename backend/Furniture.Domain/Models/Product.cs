using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Product
    {
        public int Id { get; set; }

        public string NameEn { get; set; } = null!;
        public string? NameAr { get; set; }
        public string DescriptionEn { get; set; } = null!;
        public string? DescriptionAr { get; set; }
        public decimal Price { get; set; }
        public int StockQuantity { get; set; }
        public bool IsAvailable { get; set; }
        //public bool IsCustomized { get; set; }
        public DateTime CreatedAt { get; set; }
        public string? EmbeddingVector { get; set; }
        //rel
        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string SellerId { get; set; } = null!;
        public ApplicationUser Seller { get; set; } = null!;

        public ICollection<ProductImage> Images { get; set; }=new List<ProductImage>();
        public ICollection<Review> Reviews { get; set; } = new List<Review>();
        public ICollection<CartItem> CartItems { get; set; } = new List<CartItem>();
        public ICollection<OrderItem> OrderItems { get; set; } = new List<OrderItem>();
        public ICollection<Favourite> Favourites { get; set; } = new List<Favourite>();

    }
}
