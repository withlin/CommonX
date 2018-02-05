//using JZTERP.Frameworks.Common.Components;
//using JZTERP.Frameworks.Common.Configurations;
//using JZTERP.Frameworks.Common.Logging;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data.Entity;
//using System.Data.Entity.Infrastructure;
//using System.Diagnostics;
//using System.Linq;

//namespace ECommon.Cache.Ignite.Store.Streamers
//{
//    public abstract class DbChangeProcessorBase : IDbChangeProcessor
//    {
//        public DbChangeProcessorBase()
//        {
//            BranchIds = ConfigurationManager.AppSettings["Branch"].Split(',').ToList();
//        }

//        public List<string> BranchIds { get; }

//        public ILogger Logger { get; set; }

//        public string CacheName { get; set; }

//        public bool Delete { get; set; }

//        public string EntityType { get; set; }

//        public bool Insert { get; set; }

//        public string Table { get; set; }

//        public bool Update { get; set; }

//        public IScope Scope { get; set; }

//        public ILoggerFactory LoggerFactory { get; set; }

//        protected Dictionary<string, object> ReadyToChangingProperties = new Dictionary<string, object>();

//        public void Execute(object entry)
//        {
//            if (Logger == null)
//            {
//                Logger = LoggerFactory.Create(GetType());
//            }
//            var cdWatch = new Stopwatch();
//            cdWatch.Start();
//            var item = entry as OggEntry;
//            try
//            {
//                OnExecute(item);
//            }
//            catch (Exception ex)
//            {
//                Console.WriteLine(ex);
//                Logger.Error(ex);
//            }
//            finally
//            {
//                cdWatch.Stop();
//            }
//            long count = 0;
//            if (item != null && DbNotificationService.dictInfo.ContainsKey(item.Table))
//            {
//                count = DbNotificationService.dictInfo[item.Table];
//                count = count + 1;
//                DbNotificationService.dictInfo[item.Table] = count;
//            }
//            if (Logger.IsTraceOn)
//                Logger.Trace(string.Format(" Ogg message of {1} {2} consume elapse {0} ms", cdWatch.ElapsedMilliseconds,
//                    CacheName, item.Op));
//        }

//        protected DbContext GetDbContext(OggEntry entry)
//        {
//            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//            {
//                var compiledModel = ObjectContainer.Current.BeginLifetimeScope().Resolve<DbCompiledModel>();
//                return new StoreDbContext(ConfigurationManager.ConnectionStrings[entry.HermesId] == null ? Setting.CONST_CONN_NAME : entry.HermesId,
//                     compiledModel);
//            }
//        }

//        protected abstract void OnExecute(OggEntry entry);

//        protected virtual Dictionary<string, object> GetChangedProperties(OggEntry entry)
//        {
//            Dictionary<string, object> changedProperties = new Dictionary<string, object>();

//            if (entry.Op == DbOperation.INSERT)
//                changedProperties = entry.After;
//            else if (entry.Op == DbOperation.DELETE)
//                changedProperties = entry.Before;
//            else if (entry.Op == DbOperation.UPDATE)
//            {
//                try
//                {
//                    if (entry.After?.Count > 0)
//                    {
//                        foreach (var item in entry.After)
//                        {
//                            if (entry.Before.ContainsKey(item.Key))
//                            {
//                                if (item.Value.ToString().Trim() == entry.Before[item.Key].ToString().Trim())
//                                    continue;
//                            }

//                            changedProperties.Add(item.Key, item.Value);
//                        }
//                    }
//                }
//                catch (KeyNotFoundException ex)
//                {
//                    ex.Data["BeforeKeys"] = string.Join(", ", entry.Before.Keys.ToArray());
//                    ex.Data["AfterKeys"] = string.Join(", ", entry.After.Keys.ToArray());
//                    throw ex;
//                }
//            }


//            return changedProperties;
//        }
//    }
//}