using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Furniture.Domain.InterfacesRepositories
{
    public interface IUnitOfWork
    {
        IGenaricRepository<TEntity, TKey> GetRepository<TEntity, TKey>() where TEntity : class;
        Task<int> SaveChangesAsync();
    }
}
