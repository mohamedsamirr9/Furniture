using Furniture.Domain.Models.Enum;

namespace Furniture.Services.Specifications.Order;

public class OrderByStatusSpecification :BaseSpecificationscs<Domain.Models.Order, int>
{
    
    public OrderByStatusSpecification(OrderStatus status)
        : base(o => o.Status == status)
    {
        AddInclude("OrderItems.Product");
        AddInclude(o => o.User);
        AddOrderByDescending(o => o.OrderDate);
    }
    public OrderByStatusSpecification(OrderStatus status, int pageIndex, int pageSize)
        : base(o => o.Status == status)
    {
        AddInclude("OrderItems.Product");
        AddInclude(o => o.User);
        AddOrderByDescending(o => o.OrderDate);
        ApplyPagination(pageSize, pageIndex);
    }
}