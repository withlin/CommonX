#region

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;
using CommonX.Cache;

#endregion

namespace CommonX.DataAccess
{
    public partial interface IReadRepository<TEntity, in TKey> 
    {
        [Caching]
        Task<TEntity> GetAsync(TKey id);
        [Caching]
        Task<IQueryable<TEntity>> GetAllAsync();
        [Caching]
        Task<IQueryable<TEntity>> GetAllPagedAsync(int page, int pageSize);
        [Caching]
        Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);
        [Caching]
        Task<IQueryable<TEntity>> FindPagedAsync(int page, int pageSize, Expression<Func<TEntity, bool>> predicate);
        [Caching]
        Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate);
        [Caching]
        Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate);
        [Caching]
        Task<int> CountAsync();
        [Caching]
        Task<int> CountAsync(Expression<Func<TEntity, bool>> criteria);
    }
}