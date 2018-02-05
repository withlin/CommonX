using CommonX.Cache.Ignite.Store.CacheLoaders;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Linq;
using System.Reflection;

namespace CommonX.Cache.Ignite.CacheLoaders
{
    public class CacheLoaderConfiguration
    {
        private static readonly object locker = new Object();
        //private static CacheLoaderConfiguration _instance = null;
        //public static CacheLoaderConfiguration instance()
        //{
        //    return CacheStoreConfiguration.Instance().Loaders;
        //}
        Dictionary<string, ICacheLoader> loaders = new Dictionary<string, ICacheLoader>();
        internal CacheLoaderConfiguration()
        {
        }
        internal ICacheLoader AddLoader(string typeValue, bool isAsync)
        {
            Func<Type, ICacheLoader> createInstance = (type) =>
            {
                if (type.GetCustomAttribute<SerializableAttribute>()==null)
                    throw new ConfigurationErrorsException("CacheLoader must be serializable:" + type.FullName);
                var ctor = type.GetConstructor(new Type[] { });
                return ctor.Invoke(new object[] { }) as ICacheLoader;
            };
            var typeDesc = typeValue.Split(',');
            var typeName = typeDesc[0];
            var assembly = Assembly.Load(typeDesc[1]);
            var ins = createInstance(assembly.GetType(typeName));
            ins.IsAsync = isAsync;
            loaders.Add(ins.TargetEntityTypeName, ins);
            return ins;
        }
        internal void AddLoader(params ICacheLoader[] loader)
        {
            Array.AsReadOnly<ICacheLoader>(loader).Select(x => { loaders.Add(x.TargetEntityTypeName, x); return 1; });
        }
        public ICacheLoader GetCacheLoader(string entityTypeFullName)
        {
            if (loaders.ContainsKey(entityTypeFullName)) return loaders[entityTypeFullName];
            else return null;
        }
        public ICacheLoader[] GetAll()
        {
            return loaders.Values.ToArray();
        }
    }
}
