using Furniture.Domain.InterfacesRepositories;
using Microsoft.EntityFrameworkCore.Query;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Services.Specifications
{
   public  abstract class BaseSpecificationscs<TEntity, Tkey> : ISpecifications<TEntity, Tkey> where TEntity : class
    {
        protected BaseSpecificationscs(Expression<Func<TEntity, bool>>? CriteriaExpression)
        {
            Criteria = CriteriaExpression;
        }

        public Expression<Func<TEntity, bool>>? Criteria { get; private set; }

        #region Sorting
        public Expression<Func<TEntity, object>>? OrderBy { get; private set; }

        public Expression<Func<TEntity, object>>? OrderByDescending { get; private set; }

        protected void AddOrderBy(Expression<Func<TEntity, object>> orderByExp) => OrderBy = orderByExp;
        protected void AddOrderByDescending(Expression<Func<TEntity, object>> orderByDescExp) => OrderByDescending = orderByDescExp;
        #endregion

        #region Pagination
        public int Take { get; private set; }

        public int Skip { get; private set; }

        public bool IsPaginated { get; set; }

        protected void ApplyPagination(int PageSize, int PageIndex)
        {
            IsPaginated = true;
            Take = PageSize;
            Skip = (PageIndex - 1) * PageSize;
        }
        #endregion

        #region Includes
        public List<Expression<Func<TEntity, object>>> IncludeExpressions { get; } = new();

      

        protected void AddInclude(Expression<Func<TEntity, object>> includeExpression)

            => IncludeExpressions.Add(includeExpression);

        public List<string> IncludeStringsExpressions { get; } = new();
        protected void AddInclude(string includeString)
            => IncludeStringsExpressions.Add(includeString);


        public List<Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>>> IncludeStrings { get; } = new();

        protected void AddInclude(Func<IQueryable<TEntity>, IIncludableQueryable<TEntity, object>> includeExpression)
            => IncludeStrings.Add(includeExpression);
        #endregion

    }
}
