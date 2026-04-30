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
        // Legacy optional link (kept for backward compatibility).
        public int? OrderId { get; set; }
        public Order? Order { get; set; }
        public ICollection<Order>? Orders { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = "EGP";
        public PaymentMethod Method { get; set; }
        public PaymentStatus Status { get; set; }

        public string? PaymobTransactionId { get; set; }
        public string? PaymobOrderId { get; set; }
        public string? MerchantOrderId { get; set; }

        public DateTime? PaidAt { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    }
}