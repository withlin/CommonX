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

    [Component(LifeStyle.Transient, ModuleLevel.Normal, Interceptor.Log | Interceptor.Caching)]
    public abstract partial class BaseReadRepository<TEntity,TKey> : IReadRepository<TEntity, TKey> where TEntity : class, new()
    {

        protected ILogger _logger;

        public BaseReadRepository(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType());

        }

        public virtual string KeyField { get; set; } = "PK";
        

        [Caching]
        public abstract TEntity Get(TKey id);

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
        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, bool isTrack = true) {
            return Find(predicate);
        }

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

        public abstract void Include<TProperty>(Expression<Func<TEntity, TProperty>> path);

        public abstract void Include(string path);

        
    }
}
