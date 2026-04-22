using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class PaymentByOrderIdSpecification : BaseSpecificationscs<Payment, int>
    {
        public PaymentByOrderIdSpecification(int orderId)
            : base(p => p.OrderId == orderId)
        {
            AddInclude(p => p.Order);
        }
    }
}