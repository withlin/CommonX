using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Cache
{
    /// <summary>
    /// 需要缓存的实体类型
    /// 缓存实体分类：
    /// 1.与数据库表做了Map映射的实体类，缓存键为默认的PK数据库主键，通过反射可以获取，
    /// 2.与数据库无关的自定义的复杂类型，此时将复杂类型标注为CacheEntity表示此类为缓存实体
    /// 3.与数据库表做了Map映射的实体类，缓存键为自定义的复杂类型，此时将类型标注为CacheEntity并指定CacheKey属性来反射缓存实体的键
    /// </summary>
    [AttributeUsage(AttributeTargets.Class)]
    public class CacheEntityAttribute : System.Attribute
    {
        public CacheEntityAttribute() : this(null)
        {
        }

        public CacheEntityAttribute(Type cacheKey)
        {
            CacheKey = cacheKey;
        }

        /// <summary>
        /// 自定义缓存键类型
        /// </summary>
        public Type CacheKey { get; set; }
    }
}
