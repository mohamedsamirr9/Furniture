using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class ProductCountSpecification : BaseSpecificationscs<Product, int>
    {
        public ProductCountSpecification(string? search)
            : base(p => string.IsNullOrWhiteSpace(search) || p.Name.Contains(search))
        {
        }
    }    
    
}
