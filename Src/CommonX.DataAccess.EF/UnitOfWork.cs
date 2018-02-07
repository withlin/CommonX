using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Storage;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.Common;
using System.Diagnostics;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace CommonX.DataAccess.EF
{
    [Obsolete("use dbcontext direct instead")]
    public class UnitOfWork : IUnitOfWork
    {
        #region Private Fields

        private static int i = 0;
        private bool _disposed;
        private DbContext _context;
        private IDbContextTransaction _transaction;

        #endregion Private Fields

        #region Constuctor
        public UnitOfWork(DbContext dataContext)
        {
            Debug.WriteLine("UnitOfWork init:" + i++);
            DbContext = dataContext;
        }

        #endregion Constuctor/Dispose

        #region Dispose
        //https://msdn.microsoft.com/library/ms244737.aspx

        // Dispose() calls Dispose(true)
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }
        // NOTE: Leave out the finalizer altogether if this class doesn't 
        // own unmanaged resources itself, but leave the other methods
        // exactly as they are. 
        ~UnitOfWork()
        {
            // Finalizer calls Dispose(false)
            Dispose(false);
        }
        // The bulk of the clean-up code is implemented in Dispose(bool)
        protected virtual void Dispose(bool disposing)
        {
            if (!_disposed)
            {
                if (disposing)
                {
                    // free other managed objects that implement
                    // IDisposable only

                    try
                    {
                        if (_context != null)
                        {
                            if (_context.Database.GetDbConnection().State == ConnectionState.Open)
                                _context.Database.CloseConnection();

                            _context.Dispose();
                            _context = null;
                        }
                        if (DbContext != null)
                        {
                            DbContext.Dispose();
                            DbContext = null;
                        }
                    }
                    catch (ObjectDisposedException)
                    {
                        // do nothing, the objectContext has already been disposed
                    }

                }

                _disposed = true;
            }
        }

        #endregion

        public DbContext DbContext { get; private set; }


        public int SaveChanges()
        {

            return DbContext.SaveChanges();
        }

        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
        {
            return DbContext.SaveChangesAsync(cancellationToken);
        }



        #region Unit of Work Transactions

        [Obsolete("use TransactionScope  instead")]
        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.Unspecified)
        {
            if (_context.Database.GetDbConnection().State != ConnectionState.Open)
            {
                _context.Database.OpenConnection();
            }

            _transaction = _context.Database.BeginTransaction(isolationLevel);
        }

        [Obsolete("use TransactionScope  instead")]
        public bool Commit()
        {
            SaveChanges();
            _transaction?.Commit();
            return true;
        }

        [Obsolete("use TransactionScope  instead")]
        public void Rollback()
        {
            if (_transaction?.GetDbTransaction() != null) _transaction.Rollback();
        }

        #endregion
    }
}
