using Furniture.Domain.Models;

namespace Furniture.Services.Specifications;

public class PaymentByMerchantOrderIdSpecification : BaseSpecificationscs<Payment, int>
{
    public PaymentByMerchantOrderIdSpecification(string merchantOrderId)
        : base(p => p.MerchantOrderId == merchantOrderId)
    {
        AddInclude(p => p.Order);
        AddInclude(p => p.Orders!);
    }
}