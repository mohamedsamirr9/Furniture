using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class ShippingRule
    {
        public int Id { get; set; }

        public int CategoryId { get; set; }
        public Category Category { get; set; } = null!;

        public string City { get; set; } = null!;

        public decimal Price { get; set; }

        // rel
        public ICollection<Order> Orders { get; set; } = new List<Order>();
    }
}
