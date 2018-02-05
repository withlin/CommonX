using Microsoft.EntityFrameworkCore;
using System;
using System.Data;
using System.Threading;
using System.Threading.Tasks;

namespace CommonX.DataAccess
{
    [Obsolete("use dbcontext direct instead")]
    public interface IUnitOfWork : IDisposable
    {
        DbContext DbContext { get; }
        int SaveChanges();

        Task<int> SaveChangesAsync(CancellationToken cancellationToken);

        void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified);
        bool Commit();
        void Rollback();
    }
}