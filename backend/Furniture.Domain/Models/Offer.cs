using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Offer
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int DeliveryDays { get; set; }

        public bool IsAccepted { get; set; }

        //rel
        public int OrderRequestId { get; set; }

        public string SellerId { get; set; } = null!;
        public ApplicationUser Seller { get; set; } = null!;

    }
}
