using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class CategoryWithProductsSpecifications : BaseSpecificationscs<Category, int>
    {
        public CategoryWithProductsSpecifications(int id) : base(c => c.Id == id)
        {
            AddInclude(c => c.Products);
        }
    }
}
