using Furniture.Domain.Models.Enum;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.Models
{
    public class Order
    {
        public int Id { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public OrderStatus Status { get; set; }
        public string? ShippingAddress { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsCustom { get; set; }

        //rel


        public string UserId { get; set; } = null!;
        public ApplicationUser User { get; set; }=null!;


        public ICollection<OrderItem>? OrderItems { get; set; }
        public ICollection<Payment> Payments { get; set; } = new List<Payment>();
        public ShippingRequest? ShippingRequest { get; set; }
        public Delivery? Delivery { get; set; }

    }
}
