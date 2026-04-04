using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class ReviewSpecifications : BaseSpecificationscs<Review, int>
    {
        public ReviewSpecifications(int productId, int pageIndex, int pageSize)
            : base(r => r.ProductId == productId)
        {
            AddInclude(r => r.User);
            AddOrderByDescending(r => r.CreatedAt);
            ApplyPagination(pageSize, pageIndex);
        }
    }
}