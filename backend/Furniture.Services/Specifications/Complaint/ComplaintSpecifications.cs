using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class ComplaintSpecifications : BaseSpecificationscs<Complaint,int>

    {
        public ComplaintSpecifications(string? status):base(c=>string.IsNullOrEmpty(status)||c.Status.ToString().ToLower()==status.ToLower())
        {
            AddInclude(c => c.User);
            AddInclude(c => c.Order);
            AddInclude("Order.OrderItems.Product");
            AddInclude("Order.Offer");
            AddInclude("Replies.Responder");
            AddOrderByDescending(c => c.CreatedAt);
        }
    }
}
