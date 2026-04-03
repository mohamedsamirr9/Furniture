using Furniture.Domain.InterfacesRepositories;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using Microsoft.EntityFrameworkCore;
using System.Threading.Tasks;

namespace Furniture.Persistence
{
    static class SpecificationEvaluator
    {
        public static IQueryable<TEntity> CreateQuery<TEntity, TKey>(IQueryable<TEntity> InputQuery, ISpecifications<TEntity, TKey> specifications) where TEntity : class
        {
            var Query = InputQuery;
            if (specifications.Criteria is not null)
            {
                Query = Query.Where(specifications.Criteria);
            }


            if (specifications.OrderBy is not null)
            {
                Query = Query.OrderBy(specifications.OrderBy);
            }
            else if (specifications.OrderByDescending is not null) 
            { 
                Query = Query.OrderByDescending(specifications.OrderByDescending);
            }


            if (specifications.IncludeExpressions is not null && specifications.IncludeExpressions.Count > 0)
            {
              
                Query = specifications.IncludeExpressions.Aggregate(Query, (CurrentQuery, IncludeExp) => CurrentQuery.Include(IncludeExp));
            }
            
            if (specifications.IncludeStrings is not null && specifications.IncludeStrings.Count > 0)
            {
                foreach (var includeString in specifications.IncludeStrings)
                {
                    Query = Query.Include(includeString);
                }
            }

            if (specifications.IsPaginated)
            {
                Query = Query.Skip(specifications.Skip).Take(specifications.Take);
            }




            return Query;
        }
    }
}
