using Furniture.Domain.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Delivery
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public string ShipperId { get; set; } = null!;
        public ApplicationUser Shipper { get; set; } = null!;
        public DeliveryStatus Status { get; set; }
        public Order Order { get; set; } = null!;
    }
}
