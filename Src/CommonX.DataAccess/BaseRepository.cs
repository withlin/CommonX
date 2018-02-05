using CommonX.Cache;
using CommonX.Components;
using CommonX.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

namespace CommonX.DataAccess
{
    /// <summary>
    ///     abstact class for Repository,main purpose of this class is for AOP
    /// </summary>
    /// <typeparam name="TEntity"></typeparam>
    /// <typeparam name="TKey"></typeparam>

    [Component(LifeStyle.Transient, ModuleLevel.Normal, Interceptor.Log | Interceptor.Caching )]
    public abstract partial class BaseRepository<TEntity,TKey> : IRepository<TEntity, TKey> where TEntity : class, new()
    {

        protected ILogger _logger;

        public BaseRepository(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType());

        }

        
        public virtual string KeyField { get;  set; } = "PK";

        public virtual string VersionField { get; set; } = "Version";

        [Caching(CachingMethod.Put)]
        public abstract TEntity Add(TEntity entity);
        

        [Caching(CachingMethod.Put)]
        public abstract TEntity Update(TEntity entity);

        [Caching(CachingMethod.Put)]
        public virtual TEntity Update(TEntity entity, Expression<Func<IUpdateConfiguration<TEntity>, object>> childMap=null)
        {
            return entity;
        }
        
        [Caching(CachingMethod.Put)]
        public virtual TEntity AddOrUpdate(TEntity entity, Expression<Func<IUpdateConfiguration<TEntity>, object>> childMap = null)
        {
            return entity;
        }

        [Caching(CachingMethod.Remove)]
        public abstract void Delete(TKey id);

        [Caching(CachingMethod.Remove)]
        public abstract void Delete(TEntity entity);

        [Caching(CachingMethod.Remove)]
        public virtual void Delete<TElement>(TEntity entity, Expression<Func<TEntity, ICollection<TElement>>> childPath) where TElement : class
        {
            
        }

        [Caching(CachingMethod.Remove)]
        public abstract void Delete(Expression<Func<TEntity, bool>> predicate);

        [Caching(CachingMethod.Remove)]

        public abstract void DeleteAll();


        /// <summary>
        /// 不支持include
        /// </summary>
        /// <param name="id"></param>
        /// <returns></returns>
        [Caching]
        public abstract TEntity Get(TKey id);

        /// <summary>
        /// 复合主键查询
        /// </summary>
        /// <param name="keyValues"></param>
        /// <returns></returns>
        [Caching]
        public virtual TEntity Get(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        [Caching]
        public abstract IQueryable<TEntity> GetAll();

        [Caching]
        public abstract IQueryable<TEntity> GetAllPaged(int page, int pageSize, string orderBy = "");

        [Caching]
        public abstract IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        [Caching]
        public abstract IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, bool isTrack = true);

        [Caching]
        public abstract IQueryable<TEntity> FindPaged(int page, int pageSize,
            Expression<Func<TEntity, bool>> predicate, string orderBy = "");

        [Caching]

        public abstract TEntity Single(Expression<Func<TEntity, bool>> predicate);
        [Caching]
        public abstract TEntity First(Expression<Func<TEntity, bool>> predicate);

        [Caching]
        public virtual TEntity First(Expression<Func<TEntity, bool>> predicate, bool isTrack = true)
        {
            return First(predicate);
        }

        [Caching]
        public abstract int Count();

        [Caching]
        public abstract int Count(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// 不能级联，要不然无法进行intercept
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="path"></param>
        public virtual void Include<TProperty>(Expression<Func<TEntity, TProperty>> path)
        {
        }

        /// <summary>
        /// 不能级联，要不然无法进行intercept
        /// </summary>
        /// <param name="path"></param>
        public virtual void Include(string path)
        {
        }

        public abstract void BulkSaveEntityForType();
        
    }
}
