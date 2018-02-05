using CommonX.Cache;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonX.Utilities
{
    public class CacheUtility
    {
        /// <summary>
        /// To generate the CacheKey
        /// </summary>
        /// <param name="keywords">Cache Key Keywords</param>
        /// <returns>CacheKey</returns>
        public static string GenerateCacheKey(params string[] keywords)
        {
            if (keywords == null)
            {
                throw new ArgumentNullException("Cache Keys");
            }

            StringBuilder sbKey = new StringBuilder();
            //sbKey.Append(CacheConfiguration.Instance.CacheKeyPrefix);
            sbKey.Append(CacheSettings.CachePrefixLinkedChar);

            foreach (string aKeyword in keywords)
            {
                if (string.IsNullOrEmpty(aKeyword))
                {
                    sbKey.Append("null");
                }
                else
                {
                    sbKey.Append(aKeyword);
                }
                sbKey.Append(CacheSettings.CacheKeySeparator);
            }

            string cacheKey = sbKey.Remove(sbKey.Length - 1, 1).ToString();

            //Logger.Info("Created Cache Key : " + cacheKey);

            return cacheKey;
        }

        public static void EnsureCacheKey(ref string key)
        {
            if (string.IsNullOrEmpty(key))
                return;
            StringBuilder sbKeyPrefix = new StringBuilder();
            //sbKeyPrefix.Append(CacheConfiguration.Instance.CacheKeyPrefix);
            sbKeyPrefix.Append(CacheSettings.CachePrefixLinkedChar);
            if (!key.StartsWith(sbKeyPrefix.ToString()))
            {
                key = GenerateCacheKey(key);
            }
        }

        /// <summary>
        /// To get the element type from a generic type.
        /// The generic type could be an array or a list.
        /// </summary>
        /// <typeparam name="T">Generic Type</typeparam>
        /// <returns>Element Type Full Name</returns>
        public static string GetCacheName<T>()
        {
            Type type = TypeUtils.GetPureType<T>();
            return type.Name;
            //return GetCacheName(type);
        }

        public static string GetCacheName(Type type)
        {
            //Get the custom cache name which is filled in the entity attribute 
            string cacheName = GetSpecifyCacheName(type);

            //If the custom cache name filled then choose it otherwise use the entity full name as cache name
            if (string.IsNullOrEmpty(cacheName))
            {
                cacheName = TypeUtils.GetTypeFullName(type);
            }

            return cacheName;
        }



        public static string GetSpecifyCacheName(Type type)
        {
            // Go check the entity attributes
            // find that one CacheObjectAttribute
            object[] attributes = type.GetCustomAttributes(typeof(CacheObjectAttribute), true);

            if (attributes != null)
            {
                object attributeObject = attributes.FirstOrDefault();

                CacheObjectAttribute attribute = attributeObject as CacheObjectAttribute;

                if (attribute != null)
                {
                    if (attribute.IsSpecifyCacheName)
                    {
                        if (string.IsNullOrEmpty(attribute.CacheName))
                        {
                            throw new ArgumentNullException("Specify Cache Name");
                        }

                        return TypeUtils.GetFormattedName(attribute.CacheName);
                    }
                }
            }

            return null;
        }
    }
}
