using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class OffersByRequestSpecification : BaseSpecificationscs<Offer, int>
    {
        public OffersByRequestSpecification(int requestId)
       : base(o => o.CustomRequestId == requestId)
        {
            AddInclude(o => o.Seller);        
        AddInclude(o => o.CustomRequest); 
        AddOrderByDescending(o => o.Price);
        }
    }
}
