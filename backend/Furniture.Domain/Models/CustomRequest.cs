using Furniture.Domain.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class CustomRequest
    {
        public int Id { get; set; }
        public string BuyerId { get; set; } = null!;
        public ApplicationUser Buyer { get; set; } = null!;

        public string Description { get; set; } = null!;
        public decimal Budget { get; set; }

        public CustomRequestStatus Status { get; set; }

        public ICollection<Offer> Offers { get; set; } = new List<Offer>();
    }
}
