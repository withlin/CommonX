using CommonX.Cache;
using CommonX.Logging;
using CommonX.Serializing;
using CommonX.Utilities;
using StackExchange.Redis;
using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq.Expressions;
using System.Text;

namespace CommonX.Cache.Redis
{
    public class RedisCache : ICache
    {
        /// <summary>
        ///     If the cache service get critical error, all reqeustes to the cache since the last error will be suspended and
        ///     sent to the app or data service directly, in a period indicated by suspendPeriodInSeconds.
        ///     After this period, the reqeusts to the cache will be treated normally.
        /// </summary>
        private static readonly int SuspendPeriodInSeconds = 30;// CacheConfiguration.Instance.FatalSuspendSeconds;

        //static private string regionName = CacheConfigurationManager.CacheConfiguration.RegionName;
        private static readonly bool isSuspendedByCriticalError = true;
        private static DateTime _suspendedTimeTo = DateTime.MinValue;
        private static int _continuousCriticalErrorOccuredCount;

        private static readonly int MaxFatalTimesBeforeSuspend = 5;//CacheConfiguration.Instance.MaxFatalTimesBeforeSuspend;

        private static readonly int retryTimesInSuspendedMode = 1;
        private static int _errorCounterToSuspend;
        private static readonly object SyncRoot = new object();
        private static readonly object SyncRoot2 = new object();
        private static readonly object SyncRoot3 = new object();
        private static readonly object SyncRoot4 = new object();
        private readonly IBinarySerializer _binarySerializer;
        private readonly ILogger _logger;
        private readonly IDatabase _redis;
        private string _cacheName;


        /// <summary>
        ///     Parameterized constructor.
        /// </summary>
        /// <param name="redisDatabase"></param>
        /// <param name="loggerFactory"></param>
        /// <param name="cacheName"></param>
        /// <param name="binarySerializer"></param>
        public RedisCache(IDatabase redisDatabase, IBinarySerializer binarySerializer, ILoggerFactory loggerFactory,
            string cacheName = "")
        {
            CacheName = cacheName;
            _redis = redisDatabase;
            _binarySerializer = binarySerializer;
            _errorCounterToSuspend = MaxFatalTimesBeforeSuspend;
            _logger = loggerFactory.Create(GetType());
        }


        public string CacheName
        {
            get { return _cacheName; }
            set
            {
                _cacheName = value;

                if (!string.IsNullOrEmpty(_cacheName))
                {
                    _cacheName = _cacheName.Replace('.', '_');
                }
            }
        }

        public string CarrierCode { get; set; }

        public bool IsCacheSuspended
        {
            get { return IsSuspended; }
        }

        public ConcurrentDictionary<string, CacheStatus> CacheStatus { get; set; }
        public IList<T> GetByExpression<T>(Expression<Func<T, bool>> expression)
        {
            throw new NotImplementedException();
        }

        public bool AddToList<T>(string key, T data, int maxSize = 0)
        {
            var retVal = true;
            if (_redis != null)
            {
                CacheUtility.EnsureCacheKey(ref key);
                var MAX_COUNT = 100;
                var retryTimes = MAX_COUNT;
                while (retryTimes > 0)
                {
                    //var m = Measure.StartNew();
                    try
                    {
                        lock (SyncRoot)
                        {
                            var list = _binarySerializer.Deserialize<List<T>>(_redis.HashGet(CacheName, key));
                            if (maxSize > 0 && list.Count > maxSize)
                            {
                                retVal = false;
                            }
                            else
                            {
                                list.Add(data);
                            }
                            _redis.HashSet(CacheName, key, _binarySerializer.Serialize(list));
                        }
                        retryTimes = 0;
                        //m.LogDuration("GetAndLock/PutAndUnlock " + key);
                    }
                    catch (Exception ex)
                    {
                        //m.LogDuration("retry - GetAndLock/PutAndUnlock exception " + key);
                        retryTimes = 0;
                        CatchCriticalError("AddToList-GetandPut-retry", _cacheName, key, ex);
                    }
                }
            }
            else
            {
                _logger.Debug("!!! DataCache is NULL!");
            }
            return retVal;
        }

        public List<T> GetAndRemoveList<T>(string key)
        {
            List<T> list = null;
            if (_redis != null)
            {
                CacheUtility.EnsureCacheKey(ref key);
                var retryTimes = 100;
                //var m = Measure.StartNew();
                while (retryTimes > 0)
                {
                    try
                    {
                        lock (SyncRoot2)
                        {
                            list = _binarySerializer.Deserialize<List<T>>(_redis.HashGet(CacheName, key));
                            _redis.HashDelete(CacheName, key);
                        }
                        retryTimes = 0;
                        //m.LogDuration("GetAndLock/Remove " + key);
                    }
                    catch (Exception ex)
                    {
                        CatchCriticalError("GetAndRemove", _cacheName, key, ex);
                        retryTimes = 0;
                    }
                }
            }
            return list;
        }

        public void RemoveItemsFromList<T>(string cacheKey, List<T> entitys)
        {
            if (_redis != null)
            {
                try
                {
                    CacheUtility.EnsureCacheKey(ref cacheKey);
                    lock (SyncRoot3)
                    {
                        var list = _binarySerializer.Deserialize<List<T>>(_redis.HashGet(CacheName, cacheKey));
                        var newGuys = new List<T>();
                        newGuys.AddRange(list);
                        foreach (object o in entitys)
                        {
                            foreach (var oneT in list)
                            {
                                if (o.Equals(oneT))
                                {
                                    newGuys.Remove(oneT);
                                }
                            }
                        }
                        _redis.HashSet(CacheName, cacheKey, _binarySerializer.Serialize(newGuys));
                    }
                    ResetCacheStatus();
                }
                catch (Exception ex)
                {
                    CatchCriticalError("RemvoeItemsFromList", _cacheName, cacheKey, ex);
                }
            }
        }


        private int GetCacheItemCount()
        {
            if (_redis != null)
            {
                return (int)_redis.HashLength(CacheName);
            }
            return 0;
        }

        #region Suspend Logging when data cache exception is thrown frequently

        public static bool IsSuspended
        {
            get
            {
                return (isSuspendedByCriticalError &&
                        _suspendedTimeTo > DateTime.UtcNow);
            }
        }

        private void CatchCriticalError(string method, string cacheName, string key, Exception ex)
        {
            AccessFailureIncrement();

            //var exe = new ASOASystemException(LogConstants.UnknownCarrierCode,
            //    JZTMessageKeys.SystemErrorMessages.RedisCacheError2, ex,
            //    new[] { method, cacheName, key, ex.Message });
            _logger.Error("JZTMessageKeys");
        }

        private void AccessFailureIncrement(int count = 1)
        {
            lock (SyncRoot4)
            {
                //Increase the error continuous count 
                _continuousCriticalErrorOccuredCount += count;
                //If the error count exceeds the CountToSkipCache
                if (_continuousCriticalErrorOccuredCount >= _errorCounterToSuspend)
                {
                    //Set the suspended time 
                    _suspendedTimeTo = DateTime.UtcNow.AddSeconds(SuspendPeriodInSeconds);
                    //And set the CountToSkipCache to CfgValue2
                    _errorCounterToSuspend = retryTimesInSuspendedMode;

                    //_logger.Error(new ASOAApplicationException(
                    //    LogConstants.UnknownCarrierCode,
                    //    JZTMessageKeys.WarnMessages.AppFabricCacheSuspended,
                    //    new[]
                    //    {
                    //        _continuousCriticalErrorOccuredCount.ToString(),
                    //        _suspendedTimeTo.ToString(CultureInfo.InvariantCulture)
                    //    }));
                    _logger.Error("JZTMessageKeys.WarnMessages.AppFabricCacheSuspended");
                }
            }
        }

        /// <summary>
        ///     Reset the cache status to normal status, this Method is supposed to be called once CacheServer is successfully
        ///     accessed.
        /// </summary>
        private void ResetCacheStatus()
        {
            if (_continuousCriticalErrorOccuredCount > 0 ||
                _suspendedTimeTo != DateTime.MinValue ||
                _errorCounterToSuspend != MaxFatalTimesBeforeSuspend)
            {
                lock (SyncRoot4)
                {
                    if (_continuousCriticalErrorOccuredCount >= _errorCounterToSuspend)
                    {
                        //_logger.Warn(new ASOAApplicationException(
                        //    LogConstants.UnknownCarrierCode,
                        //    JZTMessageKeys.WarnMessages.AppFabricCacheRestored));
                        _logger.Warn("JZTMessageKeys.WarnMessages.AppFabricCacheRestored");
                    }
                    //Reset the error count to 0
                    _continuousCriticalErrorOccuredCount = 0;
                    //Reset the suspended time as Min;
                    _suspendedTimeTo = DateTime.MinValue;
                    //Reset the CountToSkip as CfgValue 1;
                    _errorCounterToSuspend = MaxFatalTimesBeforeSuspend;
                }
            }
        }

        #endregion

        #region ICache Members

        public void Add(string key, object data)
        {
            if (IsSuspended)
            {
                return;
            }
            if (_redis != null)
            {
                var retryTimes = 10;
                var counter = 0;
                //var m = Measure.StartNew();
                CacheUtility.EnsureCacheKey(ref key);
                while (retryTimes > 0)
                {
                    counter++;
                    try
                    {
                        _redis.HashSet(CacheName, key, _binarySerializer.Serialize(data));

                        ResetCacheStatus();
                        retryTimes = 0;
                        //m.LogDuration(counter + "Added ");
                    }
                    catch (Exception ex)
                    {
                        retryTimes = 0;
                        CatchCriticalError("Add", _cacheName, key, ex);
                    }
                }
            }
        }


        public void Put(string key, object data)
        {
            if (IsSuspended)
            {
                return;
            }

            if (_redis != null)
            {
                try
                {
                    CacheUtility.EnsureCacheKey(ref key);
                    _redis.HashSet(CacheName, key, _binarySerializer.Serialize(data));
                    ResetCacheStatus();
                }
                catch (Exception ex)
                {
                    CatchCriticalError("Put", _cacheName, key, ex);
                }
            }
        }

        public bool Contains(string key)
        {
            CacheUtility.EnsureCacheKey(ref key);
            return Get(key) != null;
        }

        public T Get<T>(string key)
        {
            var data = default(T);

            if (IsSuspended)
            {
                return data;
            }

            if (_redis != null)
            {
                try
                {
                    CacheUtility.EnsureCacheKey(ref key);
                    var dataObj = _binarySerializer.Deserialize(_redis.HashGet(CacheName, key), typeof(T));

                    if (dataObj != null)
                    {
                        data = (T)dataObj;
                    }
                    ResetCacheStatus();
                }
                catch (Exception ex)
                {
                    CatchCriticalError("GetData", _cacheName, key, ex);
                }
            }

            return data;
        }

        public object Get(string key)
        {
            if (IsSuspended)
            {
                return null;
            }
            if (_redis != null)
            {
                try
                {
                    CacheUtility.EnsureCacheKey(ref key);
                    var redisValue = _redis.HashGet(CacheName, key);
                    ///if (string.IsNullOrEmpty(redisValue)) return null;
                    if (!redisValue.HasValue) return null;
                    var data = _binarySerializer.Deserialize<object>(redisValue);
                    ResetCacheStatus();
                    return data;
                }
                catch (Exception ex)
                {
                    CatchCriticalError("GetData", _cacheName, key, ex);
                }
            }
            return null;
        }

        public T GetAndRemove<T>(string key)
        {
            var data = default(T);
            if (IsSuspended)
            {
                return data;
            }

            if (_redis != null)
            {
                try
                {
                    CacheUtility.EnsureCacheKey(ref key);
                    data = (T)_binarySerializer.Deserialize(_redis.HashGet(CacheName, key), typeof(T));

                    if (data != null)
                    {
                        _redis.HashDelete(CacheName, key);
                        ResetCacheStatus();
                    }
                }
                catch (Exception ex)
                {
                    CatchCriticalError("GetAndRemove", _cacheName, key, ex);
                }
            }
            return data;
        }

        public void Remove(string key)
        {
            if (IsSuspended)
            {
                return;
            }

            if (_redis != null)
            {
                try
                {
                    CacheUtility.EnsureCacheKey(ref key);
                    _redis.HashDelete(CacheName, key);
                    ResetCacheStatus();
                }
                catch (Exception ex)
                {
                    CatchCriticalError("Remove", _cacheName, key, ex);
                }
            }
        }

        public void Add<TK, TV>(TK key, TV value)
        {
            throw new NotImplementedException();
        }

        public void Put<TK, TV>(TK key, TV value)
        {
            throw new NotImplementedException();
        }

        public object this[string key]
        {
            get
            {
                CacheUtility.EnsureCacheKey(ref key);
                return Get(key);
            }
        }

        public int Count
        {
            get { return GetCacheItemCount(); }
        }

        public void Flush()
        {
            _redis.KeyDelete(CacheName);
        }

        /// <summary>
        ///     To clear the cached data in the region
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        public void Flush<T>()
        {
            if (_redis != null)
            {
                Flush();
            }
        }

        public object GetByExpression<T>(object exp)
        {
            throw new NotImplementedException();
        }

        #endregion
    }
}
