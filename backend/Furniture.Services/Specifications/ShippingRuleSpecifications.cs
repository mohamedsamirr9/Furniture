using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class ShippingRuleSpecifications : BaseSpecificationscs<ShippingRule,int>
    {
        public ShippingRuleSpecifications(string? city, int? categoryId)
        : base(r =>
            (string.IsNullOrEmpty(city) || r.City.ToLower() == city.ToLower()) &&
            (!categoryId.HasValue || r.CategoryId == categoryId))
        {
            AddInclude(r => r.Category);
        }

        public ShippingRuleSpecifications(int id)
            : base(r => r.Id == id)
        {
            AddInclude(r => r.Category);
        }
    }
}
