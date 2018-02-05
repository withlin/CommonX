using CommonX.Serializing;
using CommonX.Utilities;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Runtime.Caching;
using System.Text;

namespace CommonX.Cache.Impl
{
    /// <summary>
    ///     Default implementation of IMemoryCache which using System.Runtime.Caching.MemoryCache.
    /// </summary>
    public class DefaultMemoryCache : ICache
    {
        private static readonly object SyncRoot = new object();
        private readonly CacheItemPolicy _policy = new CacheItemPolicy();
        private readonly IBinarySerializer _binarySerializer;
        private MemoryCache _dataCache;
        private readonly ICacheConfiguration _cacheConfiguration;

        public DefaultMemoryCache(IBinarySerializer binarySerializer, ICacheConfiguration cacheConfiguration)
        {
            _binarySerializer = binarySerializer;
            CacheStatus = new ConcurrentDictionary<string, CacheStatus>();
            _cacheConfiguration = cacheConfiguration;
        }

        public MemoryCache DataCache
        {
            get { return _dataCache ?? (_dataCache = new MemoryCache(_cacheName)); }
        }

        public void Flush<T>()
        {
            Flush();
        }


        public bool AddToList<T>(string key, T data, int maxSize = 0)
        {
            lock (SyncRoot)
            {
                var theList = Get<List<T>>(key);
                if (theList == null)
                {
                    theList = new List<T>();
                }
                theList.Add(data);
                Put(key, theList);
            }
            return true;
        }

        public List<T> GetAndRemoveList<T>(string key)
        {
            lock (SyncRoot)
            {
                var theList = Get<List<T>>(key);
                Remove(key);
                return theList;
            }
        }


        public void RemoveItemsFromList<T>(string cacheKey, List<T> entitys)
        {
            lock (SyncRoot)
            {
                var theList = Get<List<T>>(cacheKey);
                var newGuys = (from object o in entitys from oneT in theList where !o.Equals(oneT) select oneT).ToList();

                Put(cacheKey, newGuys);
            }
        }

        public bool IsCacheSuspended { get; internal set; }

        #region ICache Members

        public void Add(string key, object value)
        {
            if (value == null)
                return;

            //var config = CacheConfiguration.Instance.CacheConfigurations.FirstOrDefault(a => TypeUtils.GetFormattedName(a.Name).Equals(_cacheName));
            var config = _cacheConfiguration.Instance.CacheConfigurations.FirstOrDefault(a => TypeUtils.GetFormattedName(a.Name).Equals(_cacheName));

            if (config != null)
            {
                _policy.AbsoluteExpiration = DateTime.Now.AddMilliseconds(config.DefaultTimeOut);
            }
            if (_policy.AbsoluteExpiration <= DateTimeOffset.Now)
                return;
            CacheUtility.EnsureCacheKey(ref key);
            var cacheItem = new CacheItem(key, _binarySerializer.Serialize(value));

            DataCache.Add(cacheItem, _policy);

            var d = new CacheStatus(key, DateTime.Now, _policy.AbsoluteExpiration);
            CacheStatus.AddOrUpdate(key, d, (k, j) => { return d; });
        }

        public void Put(string key, object value)
        {
            CacheUtility.EnsureCacheKey(ref key);
            DataCache.Remove(key);
            Add(key, value);
        }

        public bool Contains(string key)
        {
            CacheUtility.EnsureCacheKey(ref key);
            return DataCache.Contains(key);
        }

        public int Count
        {
            get { return (int)DataCache.GetCount(); }
        }

        public void Flush()
        {
            DataCache.Dispose();
        }

        public object Get(string key)
        {
            CacheUtility.EnsureCacheKey(ref key);

            var dataObj = DataCache.Get(key);

            return dataObj == null ? null : _binarySerializer.Deserialize<object>(dataObj as byte[]);
        }

        public T Get<T>(string key)
        {
            CacheUtility.EnsureCacheKey(ref key);
            var dataObj = Get(key);

            return dataObj == null ? default(T) : (T)dataObj;
        }

        public T GetAndRemove<T>(string key)
        {
            var data = (T)Get(key);
            Remove(key);
            return data;
        }

        public void Remove(string key)
        {
            CacheUtility.EnsureCacheKey(ref key);
            DataCache.Remove(key);

        }

        public void Add<TK, TV>(TK key, TV value)
        {
            throw new NotImplementedException();
        }

        public void Put<TK, TV>(TK key, TV value)
        {
            throw new NotImplementedException();
        }

        public void LoadCache()
        {
            throw new NotImplementedException();
        }

        public IList<object> GetByExpression(Expression<Func<object, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public object this[string key]
        {
            get { return Get(key); }
        }

        private string _cacheName;

        public string CacheName
        {
            get { return _cacheName; }
            set
            {
                _cacheName = value;

                _policy.RemovedCallback = (cacheItem) =>
                {
                    CacheStatus d;
                    CacheStatus.TryRemove(cacheItem.CacheItem.Key, out d);
                };
            }
        }

        public string CarrierCode { get; set; }

        public ConcurrentDictionary<string, CacheStatus> CacheStatus { get; set; }
        public IList<T> GetByExpression<T>(Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public object GetByExpression<T>(object exp)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
