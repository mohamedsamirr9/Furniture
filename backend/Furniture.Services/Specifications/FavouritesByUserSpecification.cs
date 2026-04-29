using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Furniture.Domain.Models;
using Microsoft.EntityFrameworkCore;

namespace Furniture.Services.Specifications
{
    public class FavouritesByUserSpecification : BaseSpecificationscs<Favourite, int>
    {
        public FavouritesByUserSpecification(string userId) : base(r => r.UserId == userId)
        {
            AddInclude(r => r.Product);

            AddInclude(r => r.Include(f => f.Product).ThenInclude(p => p.Category));

            AddInclude(r => r.Include(f => f.Product).ThenInclude(p => p.Seller));

            AddInclude(r => r.Include(f => f.Product).ThenInclude(p => p.Images));
        }
    }
}
