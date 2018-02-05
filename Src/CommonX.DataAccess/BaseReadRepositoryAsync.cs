using CommonX.Cache;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

namespace CommonX.DataAccess
{
    /// <summary>
    ///     abstact class for Repository,main purpose of this class is for AOP
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>
    
    public abstract partial class BaseReadRepository<TEntity,TKey>
    {
        [Caching]
        public abstract Task<TEntity> GetAsync(TKey id);

        [Caching]
        public abstract Task<IQueryable<TEntity>> GetAllAsync();

        [Caching]
        public abstract Task<IQueryable<TEntity>> GetAllPagedAsync(int page, int pageSize);

        [Caching]
        public abstract Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate);

        [Caching]
        public abstract Task<IQueryable<TEntity>> FindPagedAsync(int page, int pageSize,
            Expression<Func<TEntity, bool>> predicate);

        [Caching]

        public abstract Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate);

        [Caching]
        public abstract Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate);

        [Caching]
        public abstract Task<int> CountAsync();

        [Caching]
        public abstract Task<int> CountAsync(Expression<Func<TEntity, bool>> criteria);

        


    }
}
