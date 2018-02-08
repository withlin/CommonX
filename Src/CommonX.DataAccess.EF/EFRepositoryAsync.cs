using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;
using System.Threading.Tasks;

namespace CommonX.DataAccess.EF
{
    public partial class EfRepository<TEntity, TKey>
    {
        public override Task AddAsync(TEntity entity)
        {
            return DbSet.AddAsync(entity);
        }
        public override Task<int> CountAsync()
        {
            return DbQuery.CountAsync();
        }

        public override Task<int> CountAsync(Expression<Func<TEntity, bool>> criteria)
        {
            return DbQuery.CountAsync(criteria);
        }
        public override Task DeleteAllAsync()
        {
            throw new NotImplementedException();
        } 

        public override Task DeleteAsync(TKey id)
        {
            throw new NotImplementedException();
        }

        public override Task DeleteAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }
        public override Task<IQueryable<TEntity>> FindAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }
        public override Task<IQueryable<TEntity>> FindPagedAsync(int page, int pageSize, Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }

        public override Task<TEntity> FirstAsync(Expression<Func<TEntity, bool>> predicate)
        {
            throw new NotImplementedException();
        }
        public override Task<IQueryable<TEntity>> GetAllAsync()
        {
            throw new NotImplementedException();
        }
        public override Task<IQueryable<TEntity>> GetAllPagedAsync(int page, int pageSize)
        {
            throw new NotImplementedException();
        }

        public override Task<TEntity> GetAsync(TKey id)
        {
            return DbSet.FindAsync(id);
        }

        public override Task<TEntity> SingleAsync(Expression<Func<TEntity, bool>> predicate)
        {
            return DbSet.SingleAsync(predicate);
        }

        public override Task UpdateAsync(TEntity entity)
        {
            throw new NotImplementedException();
        }
    }
}
