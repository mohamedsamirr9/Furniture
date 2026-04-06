using Furniture.Domain.Models.Enum;

namespace Furniture.Services.Specifications.Order;

public class OrderSpecifications : BaseSpecificationscs<Domain.Models.Order, int>
{
    #region User

    public OrderSpecifications(string userId) : base(o => o.UserId == userId)
    {
        AddInclude("OrderItems.Product");
        AddOrderByDescending(o => o.OrderDate);
    }
    public OrderSpecifications(string userId, int pageIndex, int pageSize) : base(o => o.UserId == userId)
    {
        AddInclude("OrderItems.Product");
        AddOrderByDescending(o => o.OrderDate);
        ApplyPagination(pageSize, pageIndex);
    }
    
    public OrderSpecifications(int orderId, string userId)
        : base(o => o.Id == orderId && o.UserId == userId)
    {
        AddInclude("OrderItems.Product");
        AddInclude(o => o.Payments!);  
    }

    public OrderSpecifications(string userId, OrderStatus status)
        : base(o => o.UserId == userId && o.Status == status)
    {
        AddInclude("OrderItems.Product");
        AddOrderByDescending(o => o.OrderDate);
    }

    #endregion

    #region Admin

    public OrderSpecifications(int orderId, bool isAdmin)
        : base(o => o.Id == orderId)
    {
        AddInclude("OrderItems.Product");
        AddInclude(o => o.User);
        AddInclude(o => o.Payments!);
    }

    #endregion
    
}