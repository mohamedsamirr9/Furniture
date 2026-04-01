using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.InterfacesRepositories
{
    public interface ISpecifications<TEntity, Tkey> where TEntity : class
    {
        // Filtering
        Expression<Func<TEntity, bool>>? Criteria { get; }

        // Includes
        List<Expression<Func<TEntity, object>>> IncludeExpressions { get; }

        // Sorting
        Expression<Func<TEntity, object>>? OrderBy { get; }
        Expression<Func<TEntity, object>>? OrderByDescending { get; }

        // Pagination
        int Take { get; }
        int Skip { get; }
        bool IsPaginated { get; set; }
    }
}
