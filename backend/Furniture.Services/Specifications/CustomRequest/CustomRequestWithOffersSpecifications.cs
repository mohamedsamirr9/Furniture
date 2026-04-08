using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class CustomRequestWithOffersSpecifications : BaseSpecificationscs<CustomRequest, int>
    {
        public CustomRequestWithOffersSpecifications(int id) : base(r=>r.Id==id)
        {
            AddInclude(r => r.Buyer);
            AddInclude(r => r.Offers);
            AddInclude("Offers.Seller");
        }
    }
}
