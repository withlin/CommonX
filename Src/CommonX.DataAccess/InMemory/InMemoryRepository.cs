#region

using CommonX.Logging;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;

#endregion

namespace CommonX.DataAccess.InMemory
{
    public partial class InMemoryRepository<TEntity> : IRepository<TEntity, long> where TEntity : class, new()
    {
        protected static ConcurrentDictionary<string, ConcurrentDictionary<long, TEntity>> Store
            = new ConcurrentDictionary<string, ConcurrentDictionary<long, TEntity>>();

        protected string DefaultBlock = "default";
        protected ILogger _logger;
        public virtual string KeyField { get; set; } = "PK";

        public InMemoryRepository(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(typeof(InMemoryRepository<TEntity>));
        }

        protected event OnAfterActionHandler AfterActionHandler;

        public ConcurrentDictionary<long, TEntity> GetBlockStore(string blockName)
        {
            if (blockName != null && DefaultBlock != blockName)
                DefaultBlock = blockName;
            return Store.GetOrAdd(DefaultBlock, new ConcurrentDictionary<long, TEntity>());
        }

        public  TEntity Add(TEntity entity)
        {
            var id = GenId();
            SetEntityKeyValue(entity, id);
            var blockStore = GetBlockStore(DefaultBlock);
            blockStore.AddOrUpdate(id, entity, (k, v) => entity);
            AfterActionHandler?.Invoke();
            return blockStore[id];
        }

        public  TEntity Update(TEntity entity)
        {
            var id = GetId(entity);
            var blockStore = GetBlockStore(DefaultBlock);
            if (blockStore.ContainsKey(id))
            {
                blockStore[id] = entity;
            }
            AfterActionHandler?.Invoke();
            return blockStore[id];
        }

        public TEntity Update(TEntity entity, Expression<Func<IUpdateConfiguration<TEntity>, object>> childMap = null)
        {
            throw new NotImplementedException();
        }

        public TEntity AddOrUpdate(TEntity entity, Expression<Func<IUpdateConfiguration<TEntity>, object>> childMap = null)
        {
            throw new NotImplementedException();
        }

        public  void Delete(long id)
        {
            var blockStore = GetBlockStore(DefaultBlock);
            TEntity entity;
            var result = blockStore.TryRemove(id, out entity);
            AfterActionHandler?.Invoke();
        }

        public  void Delete(TEntity entity)
        {
            throw new NotImplementedException();
        }

        public void Delete<TElement>(TEntity entity, Expression<Func<TEntity, ICollection<TElement>>> childPath) where TElement : class
        {
            throw new NotImplementedException();
        }

        public  void Delete(Expression<Func<TEntity, bool>> predicate)
        {
        }

        public  void DeleteAll()
        {
            Store.Clear();
            AfterActionHandler?.Invoke();
        }

        public void Include<TProperty>(Expression<Func<TEntity, TProperty>> path)
        {
            throw new NotImplementedException();
        }

        public void Include(string path)
        {
            throw new NotImplementedException();
        }

        public  TEntity Get(long id)
        {
            var blockStore = GetBlockStore(DefaultBlock);
            TEntity result;
            blockStore.TryGetValue(id, out result);
            return result;
        }

        public TEntity Get(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

        public  IQueryable<TEntity> GetAll()
        {
            return GetBlockStore(DefaultBlock).Values.AsQueryable();
        }

        public  IQueryable<TEntity> GetAllPaged(int page, int pageSize, string orderBy = "")
        {
            return GetBlockStore(DefaultBlock).Values.AsQueryable().Skip((page - 1)*pageSize)
                .Take(pageSize);
        }

        public  IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate)
        {
            return GetBlockStore(DefaultBlock).Values.AsQueryable().Where(predicate);
        }

        public IQueryable<TEntity> Find(Expression<Func<TEntity, bool>> predicate, bool isTrack=true)
        {
            return Find(predicate);
        }

        public  IQueryable<TEntity> FindPaged(int page, int pageSize, Expression<Func<TEntity, bool>> predicate,
            string orderBy = "")
        {
            return Find(predicate)
                .Skip((page - 1)*pageSize)
                .Take(pageSize);
        }

        public  TEntity Single(Expression<Func<TEntity, bool>> predicate)
        {
            return Find(predicate)
                .SingleOrDefault();
        }

        public  TEntity First(Expression<Func<TEntity, bool>> predicate)
        {
            return Find(predicate)
                .FirstOrDefault();
        }
        public TEntity First(Expression<Func<TEntity, bool>> predicate, bool isTrack = true)
        {
            return First(predicate);
        }

        public  int Count()
        {
            return GetBlockStore(DefaultBlock).Count;
        }

        public  int Count(Expression<Func<TEntity, bool>> criteria)
        {
            return Find(criteria)
                .Count();
        }

        private long GenId()
        {
            var blockStore = GetBlockStore(DefaultBlock);
            return blockStore.Any() ? Convert.ToInt32(blockStore.Max(c => c.Key)) + 1 : 1;
        }

        private long GetId(TEntity entity)
        {
            foreach (var p in typeof (TEntity).GetProperties())
            {
                if (p.Name == KeyField)
                    return (long) p.GetValue(entity, null);
            }
            return default(long);
        }

        private void SetEntityKeyValue(TEntity entity, long keyValue)
        {
            foreach (var p in typeof (TEntity).GetProperties())
            {
                if (p.Name == KeyField)
                {
                    var propertyValue = (long) p.GetValue(entity, null);
                    if (propertyValue == 0)
                    {
                        p.SetValue(entity, keyValue);
                    }
                    break;
                }
            }
        }

        public void BulkSaveEntityForType()
        {
            throw new NotImplementedException();
        }

        protected delegate void OnAfterActionHandler();
    }
}