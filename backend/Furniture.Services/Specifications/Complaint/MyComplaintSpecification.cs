using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class MyComplaintSpecification: BaseSpecificationscs<Complaint,int>
    {
        public MyComplaintSpecification(string userId):base (c=>c.UserId==userId)
        {
            AddOrderByDescending(c => c.CreatedAt);
        }
    }
}
