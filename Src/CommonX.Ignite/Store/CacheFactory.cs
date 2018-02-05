//using Apache.Ignite.Core.Cache.Store;
//using Apache.Ignite.Core.Common;
//using ECommon.Cache.Ignite.Store;
//using System;

//namespace JECommon.Cache.Ignite.Store
//{
//    [Serializable]
//    public class CacheFactory : IFactory<ICacheStore>
//    {
//        public ICacheStore CreateInstance()
//        {
//            return new CacheStore();
//        }
//    }

//    [Serializable]
//    public class CacheFactoryForV2<TK, TV> : IFactory<ICacheStore<TK, TV>>
//    {
//        public ICacheStore<TK, TV> CreateInstance()
//        {
//            return new CacheStoreAdapter<TK, TV>();
//        }
//    }
//}
