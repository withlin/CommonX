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
    
    public abstract partial class BaseRepository<TEntity,TKey>
    {
        

        [Caching(CachingMethod.Put)]
        public abstract Task AddAsync(TEntity entity);

        [Caching(CachingMethod.Put)]
        public abstract Task UpdateAsync(TEntity entity);

        [Caching(CachingMethod.Remove)]
        public abstract Task DeleteAsync(TKey id);

        [Caching(CachingMethod.Remove)]
        public abstract Task DeleteAsync(Expression<Func<TEntity, bool>> predicate);

        [Caching(CachingMethod.Remove)]

        public abstract Task DeleteAllAsync();

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
