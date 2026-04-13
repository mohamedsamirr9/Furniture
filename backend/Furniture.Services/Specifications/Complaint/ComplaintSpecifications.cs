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
            AddOrderByDescending(c => c.CreatedAt);
        }
    }
}
