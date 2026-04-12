using Furniture.Domain.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
    public class CategoryCountSpecification : BaseSpecificationscs<Category, int>
    {
        public CategoryCountSpecification(string? search): base(c => string.IsNullOrEmpty(search) || c.NameEn.ToLower().Contains(search.ToLower()))
        {

        }
    }
}
