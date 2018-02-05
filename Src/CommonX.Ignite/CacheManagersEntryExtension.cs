//using Apache.Ignite.Core.Cache.Configuration;
//using Apache.Ignite.Core.Cache.Store;
//using Apache.Ignite.Core.Common;
//using ECommon.Cache.Configuration;
//using ECommon.Cache.Ignite.Store;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Text;
//using System.Threading.Tasks;
//using IgniteConf = Apache.Ignite.Core.Cache.Configuration;

//namespace ECommon.Cache.Ignite
//{
//    public static class CacheManagersEntryExtension
//    {
//        public static IgniteConf.CacheConfiguration AsIgniteConfiguration(this CacheManagersEntry entry)
//        {
//            if (!entry.AllCache)
//                return null;

//            Type cacheType = Type.GetType(entry.Name);
//            Type keyType = string.IsNullOrEmpty(entry.Key) ? null : Type.GetType(entry.Key);
//            Type valueType = Type.GetType(entry.Name);

//            QueryEntity queryEntity = new QueryEntity(keyType, valueType);
//            List<QueryEntity> queryEntities = new List<QueryEntity>();
//            queryEntities.Add(queryEntity);

//            List<QueryField> queryFields = new List<QueryField>();

//            IgniteConf.CacheConfiguration conf = new IgniteConf.CacheConfiguration(entry.Name, cacheType);
//            conf.Backups = 0;
//            conf.KeepBinaryInStore = true;
//            conf.QueryEntities = queryEntities;

//            if(!keyType.IsPrimitive)
//            {
//                //缓存键为自定义类型，需要使用cachestore加载缓存
//                var factory = CacheStoreConfiguration.Instance().CacheFactory;
//                var factoryType = Type.GetType(factory);
//                var genericType = factoryType.MakeGenericType(keyType, cacheType);
//                conf.CacheStoreFactory = (IFactory<ICacheStore>)Activator.CreateInstance(genericType);
//            }

//            conf.QueryEntities = queryEntities;

//            throw new NotImplementedException();
//        }
//    }
//}
