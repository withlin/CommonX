//using Apache.Ignite.Core.Cache.Store;
//using System;
//using System.Collections.Generic;
//using System.Linq;
//using System.Threading.Tasks;
//using System.Collections;
//using System.Configuration;
//using System.Reflection;
//using ECommon.Cache.Ignite.CacheLoaders;
//using ECommon.Cache.Ignite.Store.CacheLoaders;

//namespace ECommon.Cache.Ignite.Store
//{
//    public class CacheStoreAdapter<TKey, TVal> : CacheStore, ICacheStore<TKey, TVal>
//    {
//        public void Delete(TKey key)
//        {
//        }

//        public void DeleteAll(IEnumerable<TKey> keys)
//        {
//        }

//        public TVal Load(TKey key)
//        {
//            return default(TVal);
//        }

//        public IEnumerable<KeyValuePair<TKey, TVal>> LoadAll(IEnumerable<TKey> keys)
//        {
//            throw new NotImplementedException();
//        }

//        public void LoadCache(Action<TKey, TVal> act, params object[] args)
//        {
//            Action<object, object> _action = (p1, p2) => {
//                TKey mP1 = (TKey)p1;
//                TVal mP2 = (TVal)p2;

//                act(mP1, mP2);
//            };
//            base.LoadCache(_action, args);
//        }

//        public void Write(TKey key, TVal val)
//        {

//        }

//        public void WriteAll(IEnumerable<KeyValuePair<TKey, TVal>> entries)
//        {

//        }
//    }

//    public class CacheStore : ICacheStore
//    {
        
//        public int[] AsyncLoading(bool isOverwrite, params Func<object, int>[] loaders)
//        {
//            var result = loaders.Select(r => Task.Factory.StartNew<int>(r, isOverwrite)).ToArray();
//            return result.Select(r => r.Result).ToArray();
//        }

//        public int[] ParallelLoading(bool isOverwrite, params Func<object, int>[] loaders)
//        {
//            var tasks = (from loader in loaders
//                         select Task.Factory.StartNew<int>(loader, isOverwrite)).ToArray();
//            Task.WaitAll(tasks);
//            return tasks.Select(t => t.Result).ToArray();
//        }
//        public string CacheName { get; set; }
//        public void LoadCache(Action<object, object> act, params object[] args)
//        {
//            //args[0]一定是一個字符串數組，来自JZTERP.Service.CacheService.Impl.CacheService的传递
//            string[] arguments = args[0] as string[];

//            LoadCacheParameter parameter = LoadCacheParameter.Parse(arguments);
//            parameter.IsOverwrite = (bool)args[1];
//            if (parameter.TypeNames.Length == 0 || parameter.CacheName == null)
//                throw new Exception("first argument must be serialized from LoaderCacheParameter.ToStringArrays");

//            CacheLoaderConfiguration config = CacheStoreConfiguration.Instance().LoaderConfig(parameter.CacheName);
//            if (config != null)
//            {
//                var matchedLoaders = (
//                    from type in parameter.TypeNames
//                    select config.GetCacheLoader(type)
//                    ).Where(x => x != null).ToArray();

//                foreach (ICacheLoader l in matchedLoaders)
//                {
//                    l.CacheAct = act;
//                }
//                var records = ParallelLoading(parameter.IsOverwrite, (from loader in matchedLoaders where !loader.IsAsync select new Func<object, int>(loader.Load)).ToArray());
//                Console.WriteLine(">>> total sync records loaded:{0}", records.Sum());
//                ///异步调用加载
//                var results = AsyncLoading(parameter.IsOverwrite, (from loader in matchedLoaders where loader.IsAsync select new Func<object, int>(loader.Load)).ToArray());
//                Console.WriteLine(">>> total async records loaded:{0}", results.Sum());
//            }
//            else
//            {
//                Console.WriteLine("!!!Warning!!!--No loaderConfig found for {0} with {1}", parameter.CacheName, parameter.IsOverwrite ? "overwrite" : "reload");
//            }
//        }

//        public void SessionEnd(bool commit)
//        {
//        }

//        public void Write(object key, object val)
//        {
//        }

//        public void WriteAll(IDictionary entries)
//        {
//        }
//    }
//}
