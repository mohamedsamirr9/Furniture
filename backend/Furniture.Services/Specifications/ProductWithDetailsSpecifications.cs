using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class ProductWithDetailsSpecifications : BaseSpecificationscs<Product, int>
    {
        public ProductWithDetailsSpecifications(int id)
            : base(p => p.Id == id)
        {
            AddInclude(p => p.Category);
            AddInclude(p => p.Seller);
            AddInclude(p => p.Images);
        }
    }
    
    
}
