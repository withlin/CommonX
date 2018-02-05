#region

using System;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommonX.Cache;
#endregion

namespace CommonX.DataAccess
{
    public partial interface IRepository<TEntity, in TKey> 
    {
        [Caching(CachingMethod.Put)]
        Task AddAsync(TEntity entity);
        [Caching(CachingMethod.Put)]
        Task UpdateAsync(TEntity entity);

        [Caching(CachingMethod.Remove)]
        Task DeleteAsync(TKey id);
        [Caching(CachingMethod.Remove)]
        Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);
        [Caching(CachingMethod.Remove)]
        Task DeleteAllAsync();
    }
}