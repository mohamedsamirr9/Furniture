using Furniture.Domain.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.shared.Dtos
{
    public class OfferDto
    {
        public int Id { get; set; }
        public decimal Price { get; set; }
        public int DeliveryDays { get; set; }
        public OfferStatus Status { get; set; }
        public string SellerId { get; set; } = null!;
    }
}
