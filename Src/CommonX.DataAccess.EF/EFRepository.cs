using CommonX.Logging;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.ChangeTracking;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Linq.Expressions;
using CommonX.Utilities;

namespace CommonX.DataAccess.EF
{
    public partial class EfRepository<TEntity, TKey> : BaseRepository<TEntity, TKey> where TEntity : class, new()
    {
        readonly string _connectionString = ConfigurationManager.ConnectionStrings["OracleDbContext"].ConnectionString;
        private static readonly object _lockObj = new object();
        protected readonly DbContext DbContext;
        protected IQueryable<TEntity> DbQuery;
        protected DbSet<TEntity> DbSet;
        public EfRepository(DbContext dbContext, ILoggerFactory loggerFactory) : base(loggerFactory)
        {
            DbContext = dbContext;
            DbSet = DbContext.Set<TEntity>();
            DbQuery = DbSet;
        }
        public override TEntity Add(TEntity entity)
        {
           return DbSet.Add(entity).Entity;
        }

        

        public override void BulkSaveEntityForType()
        {
            throw new NotImplementedException();
        }

        public override int Count()
        {
           return DbQuery.Count();
        }

        public override int Count(Expression<Func<TEntity, bool>> criteria)
        {
           return  DbQuery.Count(criteria);
        }

        

        public override void Delete(TKey id)
        {
            var entity = Get(id);
            if (entity != null)
                DbContext.Entry(entity).State = EntityState.Deleted;
        }

        public override void Delete(TEntity entity)
        {
            var state = DbContext.Entry(entity).State;
            if (state == EntityState.Detached)
                DbSet.Attach(entity);
            DbSet.Remove(entity);
        }

        public override void Delete(Expression<Func<TEntity, bool>> predicate)
        {
            var entities = DbSet.Where(predicate);
            DbSet.RemoveRange(entities);
        }

        public override void DeleteAll()
        {
            DbSet.RemoveRange(DbSet);
        }

        

        public override IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return DbQuery.Where(predicate);
        }

        public override IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, bool isTrack = true)
        {
            if (isTrack) return Find(predicate);
            return DbQuery.AsNoTracking().Where(predicate);
        }

        

        public override IQueryable<TEntity> FindPaged(int page, int pageSize, Expression<Func<TEntity, bool>> predicate, string orderBy = "")
        {
            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "PK";
            }
            return DbQuery.Where(predicate).OrderBy(orderBy).Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking();
        }
         
        

        public override TEntity First(Expression<Func<TEntity, bool>> predicate)
        {
            return DbQuery.FirstOrDefault(predicate);
        }

        

        public override TEntity Get(TKey id)
        {
           return  DbSet.Find(id);
        }

        public override IQueryable<TEntity> GetAll()
        {
            return DbQuery.AsNoTracking();
        }

        

        public override IQueryable<TEntity> GetAllPaged(int page, int pageSize, string orderBy = "")
        {
            if (string.IsNullOrEmpty(orderBy))
            {
                orderBy = "PK";
            }
            return DbQuery.OrderBy(orderBy).Skip((page - 1) * pageSize).Take(pageSize).AsNoTracking();
        }

        

        public override TEntity Single(Expression<Func<TEntity, bool>> predicate)
        {
            return DbQuery.SingleOrDefault(predicate);
        }



        public override TEntity Update(TEntity entity)
        {
            var entry = DbContext.Entry(entity);


            if (entry.State == EntityState.Detached)
            {
                var en = DbSet.Find(GetKeyValue(entry));
                if (en == null)
                    Ensure.NotNull(en, "update entity");
                DbContext.Entry(en).CurrentValues.SetValues(entity);
                return en;
            }
            return entity;
        }
        public override TEntity AddOrUpdate(TEntity entity,
            Expression<Func<IUpdateConfiguration<TEntity>, object>> childMap = null)
        {
            throw new NotImplementedException();
        }


        #region private method

        private static readonly IDictionary<Type, string[]> KeyFields =
            new Dictionary<Type, string[]>();

        private object[] GetKeyValue(EntityEntry entry)
        {
            IEnumerable<string> keys;
            if (KeyFields.ContainsKey(typeof(TEntity)))
            {
                keys = KeyFields[typeof(TEntity)];
            }
            else
            {
                keys = (DbContext).GetPrimaryKeyFieldsFor(typeof(TEntity)).Select(c => c.Name);
                KeyFields.Add(typeof(TEntity), keys.ToArray());
            }

            return keys.Select(k => entry.Property(k).CurrentValue).ToArray();
        }
        #endregion
    }
}
