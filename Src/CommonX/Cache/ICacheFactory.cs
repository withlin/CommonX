using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Cache
{
    public interface ICacheFactory
    {
        ICache GetCache<T>(string carrierCode);
        ICache GetCache(string carrierCode, string cacheName);
        ICache GetCache(string carrierCode, Type cacheType);
        ICache GetCache<TK, TV>(string carrierCode);
        ICache GetCache<TK, TV>(string carrierCode, string cacheName);
        Dictionary<string, ConcurrentDictionary<string, CacheStatus>> GetCacheStatus();
        bool DelCache(string cacheName);
        bool DelCacheItem(string cacheName, string key);
    }
}
