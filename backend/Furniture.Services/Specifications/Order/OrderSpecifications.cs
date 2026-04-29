using Furniture.Domain.Models.Enum;

namespace Furniture.Services.Specifications.Order;

public class OrderSpecifications : BaseSpecificationscs<Domain.Models.Order, int>
{
    #region User

    public OrderSpecifications(string userId) : base(o => o.UserId == userId)
    {
        AddInclude("OrderItems.Product.Images");
        AddInclude(o => o.Payment!);
        AddOrderByDescending(o => o.OrderDate);
    }
    public OrderSpecifications(string userId, int pageIndex, int pageSize) : base(o => o.UserId == userId)
    {
        AddInclude("OrderItems.Product.Images");
        AddInclude(o => o.Payment!);
        AddOrderByDescending(o => o.OrderDate);
        ApplyPagination(pageSize, pageIndex);
    }
    
    public OrderSpecifications(int orderId, string userId)
        : base(o => o.Id == orderId && o.UserId == userId)
    {
        AddInclude("OrderItems.Product.Images");
        AddInclude(o => o.Payment!);  
    }

    public OrderSpecifications(string userId, OrderStatus status)
        : base(o => o.UserId == userId && o.Status == status)
    {
        AddInclude("OrderItems.Product.Images");
        AddInclude(o => o.Payment!);
        AddOrderByDescending(o => o.OrderDate);
    }

    #endregion

    #region Admin

    public OrderSpecifications(int orderId, bool isAdmin)
        : base(o => o.Id == orderId)
    {
        AddInclude("OrderItems.Product.Images");
        AddInclude(o => o.User);
        AddInclude(o => o.Payment!);
    }

    #endregion
    public OrderSpecifications(int orderId)
        : base(o => o.Id == orderId)
    {
        AddInclude("OrderItems.Product.Images");
        AddInclude(o => o.Payment!);
    }
    
}