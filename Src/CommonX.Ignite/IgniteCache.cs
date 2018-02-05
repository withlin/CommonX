//using System;
//using System.Collections.Concurrent;
//using System.Collections.Generic;
//using System.Diagnostics;
//using System.Globalization;
//using System.Linq;
//using System.Linq.Expressions;
//using System.Reflection;
//using Apache.Ignite.Core;
//using Apache.Ignite.Core.Cache;
//using Apache.Ignite.Linq;
//using ECommon.Cache.Ignite.CommonCache;
//using ECommon.Logging;
//using ECommon.Utilities;
//using FastMember;
//using JZTERP.Common.Shared.LoggingSupport.Common;
//using JZTERP.Frameworks.Common.Cache.Ignite.CommonCache;
//using JZTERP.Frameworks.Common.Exceptions;
//using JZTERP.Frameworks.Common.Extensions;
//using JZTERP.Frameworks.Common.FastMember;
//using JZTERP.Frameworks.Common.Logging;
//using JZTERP.Frameworks.Common.Monitoring;
//using JZTERP.Frameworks.DataAccess.EF;

//namespace ECommon.Cache.Ignite
//{
//    public class IgniteCache<TK, TV> : ICache
//    {
//        #region constructor

//        public IgniteCache(IIgnite ignite, ILoggerFactory loggerFactory)
//        {
//            _ignite = ignite;
//            _logger = loggerFactory.Create(GetType());
//            CacheName = CacheUtility.GetCacheName<TV>();
//            _dataCache = _ignite.GetOrCreateCache<TK, TV>(CacheName);
//            _assignment = new Assignment();
//            _cacheNames = ignite.GetCacheNames()
//                .ToArray();
//        }

//        #endregion

//        #region members

//        private static readonly int SuspendPeriodInSeconds = 30;
//        private static readonly object SyncRoot = new object();
//        private static readonly bool isSuspendedByCriticalError = true;
//        private static DateTime _suspendedTimeTo = DateTime.MinValue;
//        private static int _continuousCriticalErrorOccuredCount;
//        private static readonly int retryTimesInSuspendedMode = 1;
//        private static int _errorCounterToSuspend;

//        private readonly ILogger _logger;
//        private readonly IIgnite _ignite;
//        private readonly Apache.Ignite.Core.Cache.ICache<TK, TV> _dataCache;
//        private IAssignment _assignment;

//        private readonly string[] _cacheNames;
//        //private readonly ICacheConfiguration _cacheConfiguration;

//        #endregion

//        #region ICache<TK, TV>接口成员

//        public TV this[TK key] => Get(key);

//        public string CacheName { get; set; }

//        public string CarrierCode { get; set; }

//        public void LoadCache()
//        {
//            _dataCache.LoadCache(null);
//        }

//        public bool IsCacheSuspended => isSuspendedByCriticalError && _suspendedTimeTo > DateTime.UtcNow;

//        public ConcurrentDictionary<string, CacheStatus> CacheStatus { get; set; }

//        public IList<T> GetByExpression<T>(Expression<Func<T, bool>> expression)
//        {
//            try
//            {
//                Expression<Func<ICacheEntry<TK, TV>, TV>> propertyAccessor = x => x.Value;

//                var entity = Expression.Parameter(typeof(ICacheEntry<TK, TV>));
//                var pa = Expression.Invoke(propertyAccessor, entity);
//                var te = Expression.Invoke(expression, pa);
//                var e = (Expression<Func<ICacheEntry<TK, TV>, bool>>)Expression.Lambda(te, entity);
//                var cache0 = _dataCache.AsCacheQueryable();
//                //var query = CompiledQuery2.Compile(() => cache0
//                //    .Where(e).Select(x=>x.Value));
//                var qry = cache0.Where(e);

//                // Cast to ICacheQueryable
//                var cacheQueryable = (ICacheQueryable)qry;
//                // Get resulting fields query
//                var fieldsQuery = cacheQueryable.GetFieldsQuery();
//                // Examine generated SQL
//                Debug.WriteLine(fieldsQuery.Sql);

//                var m = Measure.StartNew();

//                var result = qry.Select(c => c.Value)
//                    .ToList() as List<T>;

//                var relations = RelationshipManager.GetEntityProperties(typeof(T));

//                if (relations.Count > 0)
//                    new NavigationManager(_cacheNames, _ignite).GetNavigationProperties(result, relations);

//                m.LogDuration($"cache-{CacheName}: GetByExpression");
//                return result;
//                ;
//            }
//            catch (Exception ex)
//            {
//                CatchCriticalError("GetByExpression", CacheName, expression.ToString(), ex);
//            }
//            return null;
//        }


//        /*
//        void GetNavigationProperties(Type entityType, List<BaseEntityObject> entities, string nameChain = "")
//        {
//            // CustMainInfo.BankInfo.AccountInfo.CustMainInfo
//            // 递归调用过程中，如果名称串包含之前的名称，则表示存在循环引用，必须退出递归过程
//            if (string.IsNullOrEmpty(nameChain))
//                nameChain = entityType.Name;

//            string[] hierarchy = nameChain.Split('.');
//            if (hierarchy.Count(h => h == hierarchy.Last()) > 1)
//                return;

//            long[] pkArray = entities.Select(e => e.PK).ToArray();
//            Dictionary<long, BaseEntityObject> dict_entities = entities.ToDictionary(e => e.PK);
//            //存在缓存集的导航属性
//            string[] navCacheNames = _ignite.GetCacheNames().Intersect(
//                CacheRelationshipManager.GetNavigationPropertyNames(entityType)
//                ).ToArray();

//            List<Task> tasks = new List<Task>();
//            foreach (string name in navCacheNames)
//            {
//                var desc = CacheRelationshipManager.GetNavigationPropertyDetail(entityType, name);
//                Func<object, long> getter = PropertyGet<long>(desc.RelationEntityType, desc.RelationKeyField);
//                Action<object, object> setter = PropertySet(entityType, desc.PropertyName);

//                tasks.Add(
//                    Task.Factory.StartNew(() =>
//                    {
//                        List<object> result = FetchNavigationPropertyCacheData(pkArray, desc);
//                        // 开始递归
//                        nameChain += string.Format(".{0}", desc.RelationEntityType.Name);
//                        GetNavigationProperties(desc.RelationEntityType, result.Cast<BaseEntityObject>().ToList(), nameChain);

//                        var group = result.GroupBy(o => getter(o));
//                        group.AsParallel().ForAll(g =>
//                        {
//                            var entity = dict_entities[g.Key];

//                            if (desc.IsCollectionProperty)
//                                setter(entity, g.ToList());
//                            else
//                                setter(entity, g.FirstOrDefault());
//                        });
//                    })
//                );
//            }
//            Task.WaitAll(tasks.ToArray());
//        }

//        List<object> FetchNavigationPropertyCacheData(long[] pkList, CacheNavigationProperty description)
//        {
//            var cache = _ignite.GetCache<long, object>(description.RelationEntityType.Name);
//            IgniteSqlQueryBuilder<long, object> builer = new IgniteSqlQueryBuilder<long, object>(cache);
//            builer.ForContain(description.RelationKeyField, "number", pkList.Cast<object>().ToArray());
//            SqlQuery query = builer.Build();
//            List<object> result = cache.Query(query).Select(e => e.Value).ToList();

//            return result;
//        }

//        Action<object, object> PropertySet(Type entityType, string propertyName)
//        {
//            if (string.IsNullOrEmpty(propertyName)) return null;

//            var propertyInfo = entityType.GetProperty(propertyName, BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);
//            var sourceParamExp = Expression.Parameter(typeof(object));
//            var propertyPramExp = Expression.Parameter(typeof(object));

//            return (Action<object, object>)Expression.Lambda(
//                Expression.Call(
//                    (Expression.Convert(sourceParamExp, entityType)),
//                    propertyInfo.SetMethod,
//                    (Expression.Convert(propertyPramExp, propertyInfo.PropertyType))
//                ),
//                sourceParamExp, propertyPramExp).Compile();
//        }
//        */

//        public void Add(TK key, TV value)
//        {
//            if (IsCacheSuspended)
//                return;
//            if (key == null)
//                throw new ArgumentNullException("key must be not null");
//            if (value == null)
//                throw new ArgumentNullException("value must be not null");
//            if (_ignite == null)
//                throw new Exception("Ignite inject failure");
//            var retryTimes = 10;
//            var m = Measure.StartNew();
//            do
//            {
//                try
//                {
//                    _dataCache.Put(key, value);

//                    retryTimes = 0;
//                    m.LogDuration($"cache-{CacheName}: Add");
//                }
//                catch (Exception ex)
//                {
//                    retryTimes = 0;
//                    CatchCriticalError("Add", CacheName, key.ToString(), ex);
//                }
//            } while (retryTimes > 0);
//        }


//        public bool Contains(TK key)
//        {
//            return _dataCache.ContainsKey(key);
//        }


//        public bool Contains(string key)
//        {
//            throw new NotImplementedException();
//        }

//        public int Count => _dataCache.GetSize();

//        public object Get(string key)
//        {
//            throw new NotImplementedException();
//        }

//        public T Get<T>(string key)
//        {
//            throw new NotImplementedException();
//        }

//        object IReadCache.this[string key]
//        {
//            get { throw new NotImplementedException(); }
//        }


//        public void Add(string key, object value)
//        {
//            throw new NotImplementedException();
//        }

//        public void Put(string key, object value)
//        {
//            throw new NotImplementedException();
//        }

//        public void Flush()
//        {
//            _dataCache.Clear();
//        }

//        public void Flush<T>()
//        {
//            throw new NotImplementedException();
//        }

//        public void Remove(string key)
//        {
//            throw new NotImplementedException();
//        }


//        public TV Get(TK key)
//        {
//            if (_ignite == null)
//                throw new Exception("Ignite inject failure");
//            var data = default(TV);
//            if (IsCacheSuspended)
//                return data;
//            try
//            {
//                var result = _dataCache.TryGet(key, out data);
//            }
//            catch (Exception ex)
//            {
//                CatchCriticalError("Get", CacheName, key.ToString(), ex);
//            }
//            return data;
//        }

//        public TV GetAndRemove(TK key)
//        {
//            if (_ignite == null)
//                throw new Exception("Ignite inject failure");
//            var data = default(TV);
//            if (IsCacheSuspended)
//                return data;
//            try
//            {
//                var result = _dataCache.GetAndRemove(key);
//                if (result.Success)
//                    data = result.Value;
//            }
//            catch (Exception ex)
//            {
//                CatchCriticalError("GetAndRemove", CacheName, key.ToString(), ex);
//            }
//            return data;
//        }

//        public void Put(TK key, TV value)
//        {
//            if (_ignite == null)
//                throw new Exception("Ignite inject failure");
//            if (key == null)
//                throw new ArgumentNullException("key");
//            if (value == null)
//                throw new ArgumentNullException("value");
//            if (IsCacheSuspended)
//                return;
//            try
//            {
//                _dataCache.Replace(key, value);
//            }
//            catch (Exception ex)
//            {
//                CatchCriticalError("Put", CacheName, key.ToString(), ex);
//            }
//        }

//        public void Remove(TK key)
//        {
//            if (_ignite == null)
//                throw new Exception("Ignite inject failure");
//            try
//            {
//                _dataCache.Remove(key);
//            }
//            catch (Exception ex)
//            {
//                CatchCriticalError("Remove", CacheName, key.ToString(), ex);
//            }
//        }

//        #endregion

//        #region utilities

//        private void CatchCriticalError(string method, string cacheName, string key, Exception ex)
//        {
//            AccessFailureIncrement();

//            var exe = new ASOASystemException(LogConstants.UnknownCarrierCode,
//                JZTMessageKeys.SystemErrorMessages.RedisCacheError2, ex,
//                new[] { method, cacheName, key, ex.Message });
//            _logger.Error(exe);
//        }

//        private void AccessFailureIncrement(int count = 1)
//        {
//            lock (SyncRoot)
//            {
//                //Increase the error continuous count 
//                _continuousCriticalErrorOccuredCount += count;
//                //If the error count exceeds the CountToSkipCache
//                if (_continuousCriticalErrorOccuredCount >= _errorCounterToSuspend)
//                {
//                    //Set the suspended time 
//                    _suspendedTimeTo = DateTime.UtcNow.AddSeconds(SuspendPeriodInSeconds);
//                    //And set the CountToSkipCache to CfgValue2
//                    _errorCounterToSuspend = retryTimesInSuspendedMode;

//                    _logger.Error(new ASOAApplicationException(
//                        LogConstants.UnknownCarrierCode,
//                        JZTMessageKeys.WarnMessages.AppFabricCacheSuspended,
//                        new[]
//                        {
//                            _continuousCriticalErrorOccuredCount.ToString(),
//                            _suspendedTimeTo.ToString(CultureInfo.InvariantCulture)
//                        }));
//                }
//            }
//        }

//        #endregion
//    }

//    public class NavigationManager
//    {
//        private readonly IIgnite _ignite;
//        private readonly string[] _cacheNames;

//        public NavigationManager(string[] cacheNames, IIgnite ignite)
//        {
//            _cacheNames = cacheNames;
//            _ignite = ignite;
//        }

//        public void GetNavigationProperties<TP>(List<TP> entities, List<Relation> navigatoins)
//        {
//            if (navigatoins.Count == 0)
//                return;

//            if (entities.Count == 0)
//                return;

//            var entityType = entities.First()
//                .GetType();
//            var entityAccess = TypeAccessor.Create(entityType);
//            var dictEntities = entities.ToDictionary(e => entityAccess[e, "PK"]);
//            var pkList = entities.Select(e => entityAccess[e, "PK"])
//                .ToArray();
//            foreach (var p in navigatoins)
//                if (_cacheNames.Contains(p.MainEntity.Name))
//                {
//                    var m = GetType()
//                        .GetMethod("SetChildProperty")
//                        .MakeGenericMethod(typeof(TP), p.MainEntity);
//                    m.Invoke(this, new object[] { p, pkList, dictEntities });
//                }
//        }

//        public void SetChildProperty<TP, TC>(Relation p, object[] pkList, Dictionary<object, TP> dictEntities)
//        {
//            var accessTp = TypeAccessor.Create(typeof(TP));
//            var accessTc = TypeAccessor.Create(typeof(TC));
//            var cacheSet = _ignite.GetCache<long, TC>(p.MainEntity.Name);
//            var builer = new IgniteSqlQueryBuilder<long, TC>(cacheSet);
//            if (p.IsCollectionProperty)
//            {  //ont to many
//                builer.ForContain(p.RelationKeyField, "number", pkList.ToArray());
//            }
//            else
//            { //one to one
//                var fkList = dictEntities.Values.Select(c => accessTp[c, p.RelationKeyField]);
//                builer.ForContain("PK", "number", fkList.ToArray());
//            }
//            var query = builer.Build();
//            var result = cacheSet.Query(query)
//                .Select(e => e.Value)
//                .ToList();

//            if (p.IsCollectionProperty)
//            {   //ont to many
//                var getter = PropertyGet<long?>(p.MainEntity, p.RelationKeyField);
//                var group = result.GroupBy(o => getter(o));
//                group.AsParallel()
//                    .ForAll(g =>
//                    {
//                        var entity = dictEntities[g.Key];
//                        accessTp[entity, p.PropertyName] = g.ToList();
//                    });
//            }
//            else
//            {   //one to one

//                var dictResult = result.ToDictionary(c => accessTc[c, "PK"]);
//                dictEntities.AsParallel().ForAll(e =>
//                {
//                    accessTp[e.Value, p.PropertyName] = dictResult[accessTp[e.Value, p.RelationKeyField]];
//                });
//            }
//            if (p.ChildrenEntity.Count > 0)
//                GetNavigationProperties(result, p.ChildrenEntity);
//        }

//        private Func<object, TProperty> PropertyGet<TProperty>(Type entityType, string propertyName)
//        {
//            var propertyInfo = entityType.GetProperty(propertyName, BindingFlags.Public | BindingFlags.Instance);
//            var sourceObjectParam = Expression.Parameter(typeof(object));
//            Expression returnExpression = Expression.Call(
//                Expression.Convert(sourceObjectParam, entityType),
//                propertyInfo.GetMethod
//            );

//            return (Func<object, TProperty>)Expression.Lambda(returnExpression, sourceObjectParam)
//                .Compile();
//        }
//    }
//}