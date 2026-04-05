using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos
{
    public class OfferCreateDto
    {
        public int OrderRequestId { get; set; }
        public decimal Price { get; set; }
        public int DeliveryDays { get; set; }
    }
}
