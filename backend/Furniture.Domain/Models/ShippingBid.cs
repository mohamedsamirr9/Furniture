using Furniture.Domain.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class ShippingBid
    {
        public int Id { get; set; }

        public int ShippingRequestId { get; set; }
        public ShippingRequest ShippingRequest { get; set; } = null!;

        public string ShipperId { get; set; } = null!;
        public ApplicationUser Shipper { get; set; } = null!;

        public decimal Price { get; set; }
        public ShippingBidStatus Status { get; set; }
    }
}
