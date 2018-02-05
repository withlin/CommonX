//using System;
//using System.Collections.Generic;
//using System.Data.Entity;
//using System.Data.Entity.Infrastructure;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;

//namespace ECommon.Cache.Ignite.Store
//{
//    public class StoreDbContext : System.Data.Entity.DbContext
//    {
//        public StoreDbContext(string conn, DbCompiledModel model) : base(conn, model)
//        {

//        }
//        protected override void Dispose(bool disposing)
//        {
//            base.Dispose(disposing);
//            if (System.Runtime.GCSettings.IsServerGC)
//            {
//                System.Runtime.GCSettings.LargeObjectHeapCompactionMode = System.Runtime.GCLargeObjectHeapCompactionMode.CompactOnce;
//            }
//        }
//    }
//}
