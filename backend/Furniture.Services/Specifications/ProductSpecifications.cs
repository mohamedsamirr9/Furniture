using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class ProductSpecifications : BaseSpecificationscs<Product, int>
    {
        public ProductSpecifications(int pageIndex, int pageSize, string? search)
            : base(p => string.IsNullOrWhiteSpace(search) || p.Name.Contains(search))
        {
            AddInclude(p => p.Category);
            AddInclude(p => p.Seller);
            AddInclude(p => p.Images);

            AddOrderBy(p => p.Name);

            ApplyPagination(pageSize, pageIndex);

        }
    }
}
            
        
    
