using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class MyCustomRequestsSpecifications : BaseSpecificationscs<CustomRequest, int>
    {
        public MyCustomRequestsSpecifications(string buyerId) : base(r=>r.BuyerId== buyerId)
        {
            AddOrderByDescending(r => r.Id);
        }
    }
}
