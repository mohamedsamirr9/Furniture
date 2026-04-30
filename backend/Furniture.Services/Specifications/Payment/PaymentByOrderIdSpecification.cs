using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class PaymentByOrderIdSpecification : BaseSpecificationscs<Payment, int>
    {
        public PaymentByOrderIdSpecification(int orderId)
            : base(p => p.OrderId == orderId || (p.Orders != null && p.Orders.Any(o => o.Id == orderId)))
        {
            AddInclude(p => p.Order);
            AddInclude(p => p.Orders!);
        }
    }
}