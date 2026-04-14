using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class ComplaintWithUserSpecification:BaseSpecificationscs<Complaint,int>
    {
        public ComplaintWithUserSpecification(int id):base(c=>c.Id==id)
        {
           AddInclude(c => c.User);
            AddInclude(c => c.Order);

        }
    }
}
