using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class UserReviewsSpecification : BaseSpecificationscs<Review, int>
    {
        public UserReviewsSpecification(string userId)
            : base(r => r.UserId == userId)
        {
        }
    }
}
