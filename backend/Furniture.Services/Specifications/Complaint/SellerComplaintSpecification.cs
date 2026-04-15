using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class SellerComplaintSpecification: BaseSpecificationscs<Complaint,int>
    {
        public SellerComplaintSpecification(string sellerId) : base (c => 
            (c.Order.OrderItems != null && c.Order.OrderItems.Any(oi => oi.Product != null && oi.Product.SellerId == sellerId)) ||
            (c.Order.Offer != null && c.Order.Offer.SellerId == sellerId))
        {
            AddInclude(c => c.User);
            AddInclude(c => c.Order);
            AddInclude("Order.OrderItems.Product");
            AddInclude("Order.Offer");
            AddOrderByDescending(c => c.CreatedAt);
        }
    }
}