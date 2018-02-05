using System;
using System.Collections.Generic;

namespace CommonX.Cache.Ignite.Store.CacheLoaders
{
    public interface ICacheLoader
    {
        /// <summary>
        /// 
        /// </summary>
        /// <param name="isOverWrite">is a boolean</param>
        /// <returns></returns>
        int Load(object isOverWrite);
        bool IsAsync { get; set; }
        Action<object, object> CacheAct { get; set; }

        List<string> Branches { get; set; }
        string TargetEntityTypeName { get; }
    }
    public interface ICommonCacheLoader
    {
        int Load(bool isOverwrite=false);
    }
    public interface ICacheLoader<TKey,TVal>: ICommonCacheLoader
    {
        bool IsAsync { get; set; }

        List<string> Branches { get; set; }
    }
}
