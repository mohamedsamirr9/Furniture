using Furniture.Domain.Models;

namespace Furniture.Services.Specifications
{
    public class OrderWithItemsSpecification : BaseSpecificationscs<Domain.Models.Order, int>
    {
        public OrderWithItemsSpecification(int orderId, string userId)
            : base(o => o.Id == orderId && o.UserId == userId)
        {
            AddInclude("OrderItems.Product");
            AddInclude("OrderItems.Seller");
            AddInclude(o => o.Payment!);
            AddInclude(o => o.User);
        }
    }
}