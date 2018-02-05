using System;
using System.Collections;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Linq.Expressions;
using System.Text;

namespace CommonX.Cache
{
    public interface IReadCache
    {
        string CarrierCode { set; }

        /// <summary>
        /// 缓存名称
        /// </summary>
        string CacheName { get; set; }
        /// <summary>
        /// 是否包含指定键对应的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Contains(string key);

        /// <summary>
        /// 缓存总数量
        /// </summary>
        int Count { get; }
        /// <summary>
        /// 根据指定键获取缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        object Get(string key);

        /// <summary>
        /// 根据指定键获取指定类缓存对象
        /// </summary>
        /// <typeparam name="T"></typeparam>
        /// <param name="key"></param>
        /// <returns></returns>
        T Get<T>(string key);
        object this[string key] { get; }
        /// <summary>
        /// 是否暂停缓存
        /// </summary>
        bool IsCacheSuspended { get; }

        /// <summary>
        /// 缓存状态信息
        /// </summary>
        ConcurrentDictionary<string, CacheStatus> CacheStatus { get; set; }

        IList<T> GetByExpression<T>(Expression<Func<T, bool>> expression);

    }
    public interface ICache : IReadCache
    {

        /// <summary>
        /// 新增一个带键值对的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Add(string key, object value);

        /// <summary>
        /// 覆盖指定键对应的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Put(string key, object value);


        /// <summary>
        /// 释放所有缓存
        /// </summary>
        void Flush();

        void Flush<T>();

        /// <summary>
        /// 移除指定键对应的缓存
        /// </summary>
        /// <param name="key"></param>
        void Remove(string key);

        ///// <summary>
        ///// 获取指定键对应的缓存并从缓存列表中移除
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //T GetAndRemove<T>(string key);

        ///// <summary>
        ///// 指定键对应的缓存如果是对象列表就增加传入的对象
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <param name="data"></param>
        ///// <param name="maxSize"></param>
        ///// <returns></returns>
        //bool AddToList<T>(string key, T data, int maxSize = 0);

        ///// <summary>
        ///// 移除指定键对应的缓存,并将指定键对应的缓存以列表形式返回
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //List<T> GetAndRemoveList<T>(string key);

        ///// <summary>
        ///// 指定键对应的缓存对象列表,如果包含有传入对象列表中的对象,则移除
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="cacheKey"></param>
        ///// <param name="entitys"></param>
        //void RemoveItemsFromList<T>(string cacheKey, List<T> entitys);

    }

    public interface ICache<in TK, TV>
    {
        string CarrierCode { set; }

        /// <summary>
        /// 缓存名称
        /// </summary>
        string CacheName { get; set; }

        void LoadCache();

        /// <summary>
        /// 新增一个带键值对的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Add(TK key, TV value);

        /// <summary>
        /// 覆盖指定键对应的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <param name="value"></param>
        void Put(TK key, TV value);

        /// <summary>
        /// 是否包含指定键对应的缓存
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        bool Contains(TK key);

        /// <summary>
        /// 缓存总数量
        /// </summary>
        int Count { get; }

        /// <summary>
        /// 释放所有缓存
        /// </summary>
        void Flush();

        /// <summary>
        /// 根据指定键获取缓存对象
        /// </summary>
        /// <param name="key"></param>
        /// <returns></returns>
        TV Get(TK key);

        /// <summary>
        /// 移除指定键对应的缓存
        /// </summary>
        /// <param name="key"></param>
        void Remove(TK key);

        TV this[TK key] { get; }

        ///// <summary>
        ///// 获取指定键对应的缓存并从缓存列表中移除
        ///// </summary>
        ///// <typeparam name="T"></typeparam>
        ///// <param name="key"></param>
        ///// <returns></returns>
        //TV GetAndRemove(TK key);

        /// <summary>
        /// 是否暂停缓存
        /// </summary>
        bool IsCacheSuspended { get; }

        IList<TV> GetByExpression(Expression<Func<TV, bool>> expression);
    }
}
