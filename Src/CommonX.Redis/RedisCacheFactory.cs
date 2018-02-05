using CommonX.Cache;
using CommonX.Logging;
using CommonX.Serializing;
using CommonX.Utilities;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Text;

namespace CommonX.Cache.Redis
{
    /// <summary>
    ///     The Data Cache Factory for creating the instance of  Cache by the pass-in service type.
    /// </summary>
    public class RedisCacheFactory : ICacheFactory
    {
        private readonly IBinarySerializer _binarySerializer;
        private readonly ILogger _logger;
        private readonly ILoggerFactory _loggerFactory;
        private readonly IDatabase _redis;
        public RedisCacheFactory(IDatabase redisDatabase, IBinarySerializer binarySerializer, ILoggerFactory loggerFactory,
            string cacheName = "")
        {
            _redis = redisDatabase;
            _loggerFactory = loggerFactory;
            _binarySerializer = binarySerializer;
            _logger = loggerFactory.Create(GetType());
        }

        public ICache GetCache<T>(string carrierCode)
        {
            var cacheName = CacheUtility.GetCacheName<T>();

            return GetCache(carrierCode, cacheName);
        }

        public ICache GetCache(string carrierCode, string cacheName)
        {
            if (string.IsNullOrEmpty(carrierCode))
            {
                throw new ArgumentNullException("CarrierCode");
            }

            if (string.IsNullOrEmpty(cacheName))
            {
                throw new ArgumentNullException("CacheName");
            }

            ICache cacheProvider = null;

            try
            {

                cacheProvider = new RedisCache(_redis, _binarySerializer, _loggerFactory, cacheName);
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("RedisCacheProvider Configuration Error - cacheName " + cacheName, ex);
            }

            return cacheProvider;
        }

        public ICache GetCache(string carrierCode, Type cacheType)
        {
            throw new NotImplementedException();
        }

        public ICache GetCache(string carrierCode, string cacheName, Type cacheType)
        {
            return GetCache(carrierCode, cacheName);
        }

        public Dictionary<string, ConcurrentDictionary<string, CacheStatus>> GetCacheStatus()
        {
            //return  _caches.ToDictionary(c=>c.Key,d=>d.Value.CacheStatus);
            return null;
        }

        public bool DelCache(string cacheName)
        {
            //if (_caches.ContainsKey(cacheName))
            //{
            //    _caches[cacheName].Flush();
            //    return true;
            //}
            return false;
        }

        public bool DelCacheItem(string cacheName, string key)
        {

            return false;
        }

        public ICache<TK, TV> GetCache<TK, TV>(string carrierCode)
        {
            throw new NotImplementedException();
        }

        public ICache<TK, TV> GetCache<TK, TV>(string carrierCode, string cacheName)
        {
            throw new NotImplementedException();
        }

        ICache ICacheFactory.GetCache<TK, TV>(string carrierCode)
        {
            throw new NotImplementedException();
        }

        ICache ICacheFactory.GetCache<TK, TV>(string carrierCode, string cacheName)
        {
            throw new NotImplementedException();
        }
    }
}
