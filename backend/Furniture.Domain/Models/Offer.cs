using Furniture.Domain.Models.Enum;
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

        public OfferStatus Status { get; set; } 

        //rel
        public int CustomRequestId { get; set; }
        public CustomRequest CustomRequest { get; set; } = null!;

        public string SellerId { get; set; } = null!;
        public ApplicationUser Seller { get; set; } = null!;

        public int? OrderId { get; set; }
        public Order? Order { get; set; }

    }
}
