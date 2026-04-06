using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class MyOffersSpecification:BaseSpecificationscs<Offer,int>
    {
        public MyOffersSpecification(string sellerId)
       : base(o => o.SellerId == sellerId)
        {
            AddInclude(o => o.Seller);
            AddInclude(o => o.CustomRequest);
            AddOrderBy(o => o.Price);
        }
    }
}
