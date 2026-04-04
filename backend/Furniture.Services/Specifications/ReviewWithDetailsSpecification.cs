using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class ReviewWithDetailsSpecification : BaseSpecificationscs<Review, int>
    {
        public ReviewWithDetailsSpecification(int id)
            : base(r => r.Id == id)
        {
            AddInclude(r => r.User);
        }
    }
}