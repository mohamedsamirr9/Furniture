using Furniture.Domain.Models;
using System.Collections.Generic;
using System.Linq;

namespace Furniture.Services.Specifications
{
    public class OffersByOrderIdsSpecification : BaseSpecificationscs<Offer, int>
    {
        public OffersByOrderIdsSpecification(List<int> orderIds) 
            : base(o => o.OrderId.HasValue && orderIds.Contains(o.OrderId.Value))
        {
            AddInclude(o => o.Seller);
            AddInclude(o => o.CustomRequest);
        }
    }
}
