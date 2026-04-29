using Furniture.Domain.Models;

namespace Furniture.Services.Specifications.Order
{
    public class AllOrdersSpecification : BaseSpecificationscs<Domain.Models.Order, int>
    {
        public AllOrdersSpecification() : base(null)
        {
            AddInclude("OrderItems.Product");
            AddInclude(o => o.User);
            AddInclude(o => o.Payment!);
            AddOrderByDescending(o => o.OrderDate);
        }

        
        public AllOrdersSpecification(int pageIndex, int pageSize) : base(null)
        {
            AddInclude("OrderItems.Product");
            AddInclude(o => o.User);
            AddInclude(o => o.Payment!);
            AddOrderByDescending(o => o.OrderDate);
            ApplyPagination(pageSize, pageIndex);
        }
    }
}