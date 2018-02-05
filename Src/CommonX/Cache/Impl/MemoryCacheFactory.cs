using CommonX.Components;
using CommonX.Serializing;
using CommonX.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Text;

namespace CommonX.Cache.Impl
{
    /// <summary>
    ///     The Data Cache Factory for creating the instance of  Cache by the pass-in service type.
    /// </summary>
    public class MemoryCacheFactory : ICacheFactory
    {
        private readonly ConcurrentDictionary<string, ICache> _caches =
            new ConcurrentDictionary<string, ICache>();

        private readonly IBinarySerializer _binarySerializer;
        private ICacheConfiguration _cacheConfiguration;

        public MemoryCacheFactory(IBinarySerializer binarySerializer)
        {
            _binarySerializer = binarySerializer;
        }


        public virtual ICache GetCache<T>(string carrierCode)
        {
            var cacheName = CacheUtility.GetCacheName<T>();

            return GetCache(carrierCode, cacheName);
        }


        public virtual ICache GetCache(string carrierCode, string cacheName)
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

                if (_caches.ContainsKey(cacheName))
                    cacheProvider = _caches[cacheName];
                else
                {
                    //ICacheConfiguration不能依赖注入，会引发循环依赖异常,也不能放在构造中Resolve这样会拿到默认的实现而不是后面注册的实现
                    using (var scope = ObjectContainer.Current.BeginLifetimeScope())
                    {
                        _cacheConfiguration = scope.Resolve<ICacheConfiguration>();
                    }

                    cacheProvider = new DefaultMemoryCache(_binarySerializer, _cacheConfiguration);
                    cacheProvider.CacheName = cacheName;
                    cacheProvider.CarrierCode = carrierCode;
                    _caches.GetOrAdd(cacheName, cacheProvider);
                }
            }
            catch (Exception ex)
            {
                throw new ConfigurationErrorsException("MemoryCacheProvider Configuration Error - cacheName " + cacheName, ex);
            }

            return cacheProvider;
        }

        public virtual ICache GetCache(string carrierCode, Type cacheType)
        {
            var cacheName = CacheUtility.GetCacheName(cacheType);
            return GetCache(carrierCode, cacheName);
        }

        public virtual ICache GetCache<TK, TV>(string carrierCode)
        {
            throw new NotImplementedException();
        }

        public virtual ICache GetCache<TK, TV>(string carrierCode, string cacheName)
        {
            throw new NotImplementedException();
        }

        public virtual Dictionary<string, ConcurrentDictionary<string, CacheStatus>> GetCacheStatus()
        {
            return _caches.ToDictionary(c => c.Key, d => d.Value.CacheStatus);
        }

        public virtual bool DelCache(string cacheName)
        {
            if (_caches.ContainsKey(cacheName))
            {
                _caches[cacheName].Flush();
                return true;
            }
            return false;
        }
        public virtual bool DelCacheItem(string cacheName, string key)
        {
            if (_caches.ContainsKey(cacheName))
            {
                _caches[cacheName].Remove(key);
                return true;
            }
            return false;
        }

    }
}
