#region

using System;
using System.Collections.Generic;
using System.Linq.Expressions;
using CommonX.Cache;

#endregion

namespace CommonX.DataAccess
{
    public partial interface IRepository<TEntity, in TKey> : IReadRepository<TEntity, TKey> where TEntity : class, new()
    {

        [Caching(CachingMethod.Put)]
        TEntity Add(TEntity entity);


        [Caching(CachingMethod.Put)]
        TEntity Update(TEntity entity);

        [Caching(CachingMethod.Put)]
        TEntity Update(TEntity entity, Expression<Func<IUpdateConfiguration<TEntity>, object>> childMap = null);
        
        [Caching(CachingMethod.Put)]
        TEntity AddOrUpdate(TEntity entity, Expression<Func<IUpdateConfiguration<TEntity>, object>> childMap = null);

        [Caching(CachingMethod.Remove)]
        void Delete(TKey id);

        [Caching(CachingMethod.Remove)]
        void Delete(TEntity entity);

        [Caching(CachingMethod.Remove)]
        void Delete<TElement>(TEntity entity, Expression<Func<TEntity, ICollection<TElement>>> childPath) where TElement : class;

        [Caching(CachingMethod.Remove)]
        void Delete(Expression<Func<TEntity, bool>> predicate);

        [Caching(CachingMethod.Remove)]
        void DeleteAll();


        void BulkSaveEntityForType();
    }
}