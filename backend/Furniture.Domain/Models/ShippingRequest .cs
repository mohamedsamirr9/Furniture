using Furniture.Domain.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class ShippingRequest
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public ShippingRequestStatus Status { get; set; }

        public ICollection<ShippingBid> Bids { get; set; } = new List<ShippingBid>();
    }
}
