using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class FavouriteByUserAndProductSpecification : BaseSpecificationscs<Favourite, int>
    {
        public FavouriteByUserAndProductSpecification(string userId, int productId) : base(r => r.UserId == userId && r.ProductId == productId)
        {
        }
    }
}

