using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;

namespace Furniture.Services.Specifications
{
    public class CategoryWithProductsSpecifications : BaseSpecificationscs<Category, int>
    {
        public CategoryWithProductsSpecifications(int id) : base(c => c.Id == id)
        {
            AddInclude(q => q.Include(c => c.Products).ThenInclude(p => p.Seller));

            AddInclude(q => q.Include(c => c.Products).ThenInclude(p => p.Images));


        }
    }
}
