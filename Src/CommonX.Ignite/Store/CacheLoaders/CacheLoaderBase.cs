//using Apache.Ignite.Core;
//using Apache.Ignite.Core.Cache;
//using JZTERP.Frameworks.Common.Components;
//using JZTERP.Frameworks.Common.Configurations;
//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Data.Entity;
//using System.Data.Entity.Infrastructure;

//namespace ECommon.Cache.Ignite.Store.CacheLoaders
//{
//    [Serializable]
//    public abstract class CacheLoaderBase<TKey, TVal> : ICacheLoader
//    {
//        public Action<object, object> CacheAct
//        {
//            get;

//            set;
//        }

//        public string TargetEntityTypeName
//        {
//            get
//            {
//                return typeof(TVal).FullName;
//            }
//        }
//        public string TargetKeyTypeName
//        {
//            get
//            {
//                return typeof(TKey).FullName;
//            }
//        }
//        public List<string> Branches { get; set; }

//        public bool IsAsync
//        {
//            get;
//            set;
//        }

//        public string GetCacheName()
//        {
//            var config = Ignition.GetIgnite().GetConfiguration();
//            foreach (var c in config.CacheConfiguration)
//            {
//                foreach (var qe in c.QueryEntities)
//                {
//                    if (qe.ValueTypeName == this.TargetEntityTypeName)
//                    {
//                        return c.Name;
//                    }
//                }
//            }
//            return null;
//        }
//        public Apache.Ignite.Core.Cache.ICache<TKey, TVal> GetCache()
//        {
//            using (var scope = ObjectContainer.BeginLifetimeScope())
//            {
//                IIgnite ignite = scope.Resolve<IIgnite>();
//                var cacheName = this.GetCacheName();
//                if (null != cacheName)
//                    return ignite.GetOrCreateCache<TKey, TVal>(cacheName);
//                else
//                    return null;
//            }
//        }
//        public int Load(object isOverwrite)
//        {
//            using (var oc = ObjectContainer.BeginLifetimeScope())
//            {
//                IIgnite ignite = oc.Resolve<IIgnite>();
//                bool overwrite = (bool)isOverwrite;
//                var typeID = ignite.GetBinary().GetBinaryType(TargetEntityTypeName).TypeId;
//                Console.WriteLine("{0} [TypeID:{1}] loading.", TargetEntityTypeName, typeID);

//                int count = 0;
//                try
//                {
//                    count = InnerLoad(oc, overwrite);
//                    Console.WriteLine("{0} [TypeID:{1}] load completed for {2} records.", TargetEntityTypeName, typeID, count);
//                }
//                catch(Exception ex)
//                {
//                    Console.WriteLine("{0} [TypeID:{1}] load failed for {2}. Please run [reload {0}]", TargetEntityTypeName, typeID, ex.Message);
//                }
//                return count;
//            }
//        }
//        protected abstract int InnerLoad(IScope objectContext, bool isOverwrite);

//        protected DbContext GetDbContext(string branchId)
//        {
//            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//            {
//                var compiledModel = ObjectContainer.Current.BeginLifetimeScope().Resolve<DbCompiledModel>();
//                return new StoreDbContext(GetConnectionStringFromBranchId(branchId),
//                     compiledModel);
//            }
//        }
//        /// <summary>
//        /// 计算从branchId，推导成HermesId，然后返回连接字符串。
//        /// </summary>
//        /// <param name="branchId">分公司简码</param>
//        /// <returns>连接字符串或配置名字</returns>
//        private string GetConnectionStringFromBranchId(string branchId)
//        {
//            string hermesId = CacheStoreConfiguration.Instance().GetHermesId(branchId);
//            return ConfigurationManager.ConnectionStrings[hermesId] == null ? Setting.CONST_CONN_NAME : hermesId;
//        }
//    }
//}
