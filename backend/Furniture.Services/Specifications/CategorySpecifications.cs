using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class CategorySpecifications : BaseSpecificationscs<Category, int>
    {
        public CategorySpecifications(int pageIndex, int pageSize, string? search) : base
        (c => string.IsNullOrWhiteSpace(search) || c.Name.Contains(search))
        {
            AddOrderBy(c => c.Name);
         
            ApplyPagination(pageSize, pageIndex);
        }
    }
}
