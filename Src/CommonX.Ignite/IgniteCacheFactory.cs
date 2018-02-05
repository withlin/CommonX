//using Apache.Ignite.Core;
//using Apache.Ignite.Core.Common;
//using ECommon.Cache.Impl;
//using ECommon.Logging;
//using ECommon.Serializing;
//using ECommon.Utilities;
//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Configuration;

//namespace ECommon.Cache.Ignite
//{
//    public class IgniteCacheFactory : MemoryCacheFactory
//    {
//        #region members

//        private readonly ILoggerFactory _loggerFactory;
//        private readonly ILogger _logger;
//        private readonly IIgnite _ignite;

//        #endregion

//        #region constructor

//        public IgniteCacheFactory(IIgnite igniteDatabase,  ILoggerFactory loggerFactory,IBinarySerializer binarySerializer) :base(binarySerializer)
//        {
//            _loggerFactory = loggerFactory;
//            _logger = loggerFactory.Create(GetType());
//            _ignite = igniteDatabase;
//        }

//        #endregion       

//        #region ICacheFactory接口成员

        
//        public override ICache GetCache(string carrierCode,Type cacheType)
//        {
//            var ms = this.GetType().GetMethods()[1];
//            var cache = ms.MakeGenericMethod(typeof(long), cacheType).Invoke(this, new []{carrierCode});
//            return (ICache)cache;
//        }

//        public override ICache GetCache<TK, TV>(string carrierCode)
//        {
//            var cacheName = CacheUtility.GetCacheName<TV>();
//            return GetCache<TK, TV>(carrierCode, cacheName);
//        }        

//        public override ICache GetCache<TK, TV>(string carrierCode, string cacheName)
//        {
//            if (string.IsNullOrEmpty(carrierCode))
//                throw new ArgumentNullException("CarrierCode");
//            if (string.IsNullOrEmpty(cacheName))
//                throw new ArgumentNullException("CacheName");
//            try
//            {
//                ICache cache = new IgniteCache<TK, TV>(_ignite, _loggerFactory);
//                cache.CarrierCode = carrierCode;

//                return cache;
//            }
//            catch (Exception ex)
//            {
//                throw new ConfigurationErrorsException("IgniteCache Error - cacheName" + cacheName, ex);
//            }
//        }
        

//        #endregion


//    }
//}