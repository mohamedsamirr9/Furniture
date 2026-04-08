using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class CustomRequestSpecifications : BaseSpecificationscs<CustomRequest, int>
    {
        public CustomRequestSpecifications(int pageIndex, int pageSize, string? status, decimal? minBudget) : base(r=>
        r.Status == Furniture.Domain.Models.Enum.CustomRequestStatus.Open &&
        !r.Offers.Any(o => o.Status == Furniture.Domain.Models.Enum.OfferStatus.Accepted) &&
        (string.IsNullOrWhiteSpace(status) || r.Status.ToString().ToLower() == status.ToLower()) &&
        (!minBudget.HasValue || r.Budget >= minBudget))
        {
            AddInclude(r => r.Buyer);
            AddInclude(r => r.Offers);

            ApplyPagination(pageSize, pageIndex);
            AddOrderByDescending(r => r.Id);
        }
    }
}
