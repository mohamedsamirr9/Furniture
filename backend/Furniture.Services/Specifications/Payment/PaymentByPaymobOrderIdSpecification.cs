using Furniture.Domain.Models;

namespace Furniture.Services.Specifications;

public class PaymentByPaymobOrderIdSpecification : BaseSpecificationscs<Payment, int>
{
    public PaymentByPaymobOrderIdSpecification(string paymobOrderId)
        : base(p => p.PaymobOrderId == paymobOrderId)
    {
        AddInclude(p => p.Order);
        AddInclude(p => p.Orders!);
    }
}