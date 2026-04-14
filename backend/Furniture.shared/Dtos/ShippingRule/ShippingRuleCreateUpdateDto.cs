using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos.ShippingRule
{
    public class ShippingRuleCreateUpdateDto
    {
        public int CategoryId { get; set; }
        public string City { get; set; } = null!;
        public decimal Price { get; set; }
    }
}
