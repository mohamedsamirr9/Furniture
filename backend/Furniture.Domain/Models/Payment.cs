using Furniture.Domain.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Payment
    {
        public int Id { get; set; }
        //public string? PaymentMethod { get; set; }
        public PaymentType Type { get; set; }
        public decimal Amount { get; set; }

        public PaymentStatus Status { get; set; }
        public DateTime PaymentDate { get; set; }

        //rel
        public int OrderId { get; set; }
        public Order Order { get; set; } = null!;
    }
}
