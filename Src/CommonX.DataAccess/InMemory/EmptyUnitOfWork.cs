//using System;
//using System.Collections.Generic;
//using System.Data;
//using System.Data.Entity;
//using System.Linq;
//using System.Text;
//using System.Threading;
//using System.Threading.Tasks;

//namespace JZTERP.Frameworks.DataAccess.InMemory
//{
//    [Obsolete("use dbcontext direct instead")]
//    public class EmptyUnitOfWork : IUnitOfWork
//    {
//        public void Dispose()
//        {

//        }

//        public DbContext DbContext { get; }
//        public int SaveChanges()
//        {
//            return 0;
//        }

//        public void BeginTransaction(IsolationLevel isolationLevel = IsolationLevel.ReadCommitted)
//        {
//        }

//        public bool Commit()
//        {
//            return true; ;
//        }

//        public void Rollback()
//        {
//        }

//        public Task<int> SaveChangesAsync(CancellationToken cancellationToken)
//        {
//            return Task.FromResult(0);
//        }
//    }
//}
