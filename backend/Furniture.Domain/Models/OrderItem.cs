using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class OrderItem
    {
        public string SellerId { get; set; } = null!;
        public ApplicationUser Seller { get; set; } = null!;

        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }

        //rel
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;

        public int ProductId { get; set; }
        public Product Product { get; set; } = null!;
    }
}
