//using Apache.Ignite.Core.Log;
//using ECommon.Components;
//using ECommon.Logging;
//using System;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Linq;
//using System.Threading.Tasks;

//namespace ECommon.Cache.Ignite.Store
//{
//    /// <summary>
//    /// 用于侦听特殊cache CDC 通知的receiver。在收到变更通知后，从系统配置中选择同步器(IDbChangeStreamer)并调用执行
//    /// </summary>

//    [Serializable]
//    public class OggNotificationReceiver : Apache.Ignite.Core.Datastream.IStreamReceiver<string, OggEntry>
//    {
//        public string ForCacheName { get; set; }

//        private Logging.ILogger Logger { get; set; }
        
//        public OggNotificationReceiver(string forCacheName)
//        {
//            this.ForCacheName = forCacheName;
//        }
//        public void Receive(Apache.Ignite.Core.Cache.ICache<string, OggEntry> cache, ICollection<Apache.Ignite.Core.Cache.ICacheEntry<string, OggEntry>> entries)
//        {
//            if (Logger == null)
//            {
//                using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//                {
//                    var LoggerFactory = scope.Resolve<ILoggerFactory>();
//                    Logger = LoggerFactory.Create(GetType());
//                }
//            }
//            Stopwatch sw = new Stopwatch();
//            sw.Start();
//            var orderred = entries.OrderBy(x => x.Value.TS);
//            foreach(var entry in orderred)
//            {
//                var insertTasks = (from handler in CacheStoreConfiguration.Instance().GetDbChangeProcessorsForInsert(ForCacheName)
//                                   where handler.Table == entry.Value.Table && entry.Value.Op== DbOperation.INSERT
//                                   select Task.Factory.StartNew(new Action<object>(handler.Execute), entry.Value)).ToArray();

//                var updateTasks = (from handler in CacheStoreConfiguration.Instance().GetDbChangeProcessorsForUpdate(ForCacheName)
//                                   where handler.Table == entry.Value.Table && entry.Value.Op == DbOperation.UPDATE
//                                   select Task.Factory.StartNew(new Action<object>(handler.Execute), entry.Value)).ToArray();

//                var deleteTasks = (from handler in CacheStoreConfiguration.Instance().GetDbChangeProcessorsForDelete(ForCacheName)
//                                   where handler.Table == entry.Value.Table && entry.Value.Op == DbOperation.DELETE
//                                   select Task.Factory.StartNew(new Action<object>(handler.Execute), entry.Value)).ToArray();
//                if(insertTasks.Length>0) Task.WaitAll(insertTasks);
//                if (updateTasks.Length > 0) Task.WaitAll(updateTasks);
//                if (deleteTasks.Length > 0) Task.WaitAll(deleteTasks);
//            }
//            sw.Stop();

//            if (Logger.IsDebugEnabled)
//            {

//            }
//                //Logger.Trace(string.Format("ProdQuery Ogg message of a batch {1}  consume elapse {0} ms", sw.ElapsedMilliseconds,
//                //    entries.Count));
//            //我们并不需要同步等待完成
//            //Task.WaitAll(tasks);
//        }
//    }

//}
