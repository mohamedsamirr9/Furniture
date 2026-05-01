using Furniture.Domain.Models;

namespace Furniture.Services.Specifications.Order;

public class OrdersByPaymentIdSpecification : BaseSpecificationscs<Domain.Models.Order, int>
{
    public OrdersByPaymentIdSpecification(int paymentId, string? userId = null)
        : base(o => o.PaymentId == paymentId && (userId == null || o.UserId == userId))
    {
        AddInclude("OrderItems.Product");
        AddInclude("OrderItems.Seller");
        AddInclude(o => o.Payment!);
        AddInclude(o => o.User);
    }
}

