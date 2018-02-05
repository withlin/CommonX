#region

using System;
using System.Linq;
using System.Linq.Expressions;
using CommonX.Cache;
#endregion

namespace CommonX.DataAccess
{
    public partial interface IReadRepository<TEntity, in TKey> where TEntity : class, new()
    {
        string KeyField { set; }
        
        [Caching]
        TEntity Get(TKey id);

        [Caching]
        TEntity Get(params object[] keyValues);

        [Caching]
        IQueryable<TEntity> GetAll();

        [Caching]
        IQueryable<TEntity> GetAllPaged(int page, int pageSize, string orderBy = "");

        [Caching]
        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate);

        [Caching]
        IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicatebool, bool isTrack=true);

        [Caching]
        IQueryable<TEntity> FindPaged(int page, int pageSize,
            Expression<Func<TEntity, bool>> predicate, string orderBy = "");

        [Caching]
        TEntity Single(Expression<Func<TEntity, bool>> predicate);

        [Caching]
        TEntity First(Expression<Func<TEntity, bool>> predicate);
        [Caching]
        TEntity First(Expression<Func<TEntity, bool>> predicate, bool isTrack = true);

        [Caching]
        int Count();

        [Caching]
        int Count(Expression<Func<TEntity, bool>> criteria);

        /// <summary>
        /// 不能级联，要不然无法进行intercept
        /// </summary>
        /// <typeparam name="TProperty"></typeparam>
        /// <param name="path"></param>
        void Include<TProperty>(Expression<Func<TEntity, TProperty>> path);

        /// <summary>
        /// 不能级联，要不然无法进行intercept
        /// </summary>
        /// <param name="path"></param>
        void Include(string path);

    }
}