#region

using System;
using System.Linq;
using System.Linq.Expressions;
using System.Threading.Tasks;

#endregion

namespace CommonX.DataAccess.InMemory
{
    public partial class InMemoryRepository<TEntity> 
    {

        public  Task AddAsync(TEntity entity)
        {
            return Task.Run(() =>
            {
                var id = GenId();
               SetEntityKeyValue(entity, id);
                var blockStore = this.GetBlockStore(DefaultBlock);
                blockStore.AddOrUpdate(id, entity, (k, v) => entity);
               AfterActionHandler?.Invoke();
               return true;
           });
        }

        public  Task UpdateAsync(TEntity entity)
        {
            return Task.Run(() =>
           {
               var blockStore = this.GetBlockStore(DefaultBlock);
               blockStore.AddOrUpdate(GetId(entity), entity, (k, v) => entity);
               AfterActionHandler?.Invoke();
               return true;
           });
        }

        public  Task DeleteAsync(long id)
        {
            return Task.Run(() =>
           {
               TEntity entity;
               var blockStore = this.GetBlockStore(DefaultBlock);
               var result = blockStore.TryRemove(id, out entity);
               AfterActionHandler?.Invoke();
               return result;
           });
        }

        public  Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Task.Run(() =>
              {
                  return false;
              });
        }

        public  Task DeleteAllAsync()
        {
            return Task.Run(() =>
              {
                  Store.Clear();
                  AfterActionHandler?.Invoke();
                  return true;
              });
        }

        public  Task<TEntity> GetAsync(long id)
        {
            return Task.Run(() =>
              {
                  TEntity result;
                  var blockStore = this.GetBlockStore(DefaultBlock);
                  blockStore.TryGetValue(id, out result);
                  return result;
              });
        }

        public  Task<IQueryable<TEntity>> GetAllAsync()
        {

            return Task.Run(() => this.GetBlockStore(DefaultBlock).Values.AsQueryable());
        }

        public  Task<IQueryable<TEntity>> GetAllPagedAsync(int page, int pageSize)
        {
            if (!Store.Any())
            {
                return null;
            }
            return Task.Run(() => this.GetBlockStore(DefaultBlock).Values.AsQueryable().Skip((page - 1) * pageSize)
               .Take(pageSize));
        }

        public  Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Task.Run(() => this.GetBlockStore(DefaultBlock).Values.AsQueryable().Where(predicate));
        }

        public  Task<IQueryable<TEntity>> FindPagedAsync(int page, int pageSize, Expression<Func<TEntity, bool>> predicate)
        {
            return Task.Run(() => (FindAsync(predicate)).Result.Skip((page - 1) * pageSize).Take(pageSize));

        }

        public  Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Task.Run(() => (FindAsync(predicate)).Result.SingleOrDefault());

        }

        public  Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return Task.Run(() => (FindAsync(predicate)).Result.FirstOrDefault());

        }

        public  Task<int> CountAsync()
        {
            return Task.Run(() => Store.Count);
        }

        public  Task<int> CountAsync(Expression<Func<TEntity, bool>> criteria)
        {
            return Task.Run(() => (FindAsync(criteria)).Result.Count());

        }

        #region IDisposable

        public  void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            if (!disposing)
            {
                return;
            }
            //_store = null;
        }

        #endregion
    }
}