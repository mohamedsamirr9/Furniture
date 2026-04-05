using Furniture.Domain.InterfacesRepositories;
using Furniture.Persistence.Data.DbContexts;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Persistence.Repositories
{
    public class UnitOfWork(FurnitureDbContext _dbContext) : IUnitOfWork
    {
        private readonly Dictionary<string, object> _repositories = [];
        public IGenaricRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class
        {
            var typeName = typeof(TEntity).Name;

            if (_repositories.TryGetValue(typeName, out object? value))

                return (IGenaricRepository<TEntity, TKey>) value;

            else
            {
                var Repo = new GenaricRepository<TEntity, TKey>(_dbContext);
                _repositories[typeName] = Repo;
                return Repo;
            }

        }

        public async Task<int> SaveChangesAsync()
        {
          return await _dbContext.SaveChangesAsync();
        }
    }
}
