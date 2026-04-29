using Furniture.Domain.Models;

namespace Furniture.Services.Specifications.Order;

/// <summary>
/// Single order for a seller: must belong to this seller via order items or offer.
/// </summary>
public class SellerOrderByIdSpecification : BaseSpecificationscs<Domain.Models.Order, int>
{
    public SellerOrderByIdSpecification(int orderId, string sellerId)
        : base(o =>
            o.Id == orderId &&
            (
                (o.OrderItems != null && o.OrderItems.Any(i => i.Product != null && i.Product.SellerId == sellerId)) ||
                (o.Offer != null && o.Offer.SellerId == sellerId)
            ))
    {
        AddInclude("OrderItems.Product.Images");
        AddInclude(o => o.User);
        AddInclude(o => o.Offer!);
        AddInclude("Offer.CustomRequest");
        AddInclude(o => o.Payment!);
    }
}
