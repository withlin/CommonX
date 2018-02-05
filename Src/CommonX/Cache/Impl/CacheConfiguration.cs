using CommonX.Cache.Configuration;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace CommonX.Cache.Impl
{
    [Serializable]
    public class CacheConfiguration : ICacheConfiguration
    {
        #region ICacheConfiguration成员

        public ICacheConfiguration Instance
        {
            get { return this; }
        }

        public string CacheKeyPrefix { get; set; } = string.Empty;

        public int TransportProperties_MaxBufferSize { get; set; } = 102400;

        public int RefreshMinutes { get; set; } = 60;

        public List<CacheManagersEntry> CacheConfigurations { get; set; } = new List<CacheManagersEntry>();

        public bool UseRedisCache { get; set; } = false;

        public bool UseFullCache { get; set; } = true;

        public string IgniteGridName { get; set; } = string.Empty;

        public long IgniteOffHeapMaxMemory { get; set; } = 0;

        public bool IgniteEnableSwap { get; set; } = false;

        public int IgniteMemoryMode { get; set; } = 1;

        public int IgniteSqlOnheapRowCacheSize { get; set; } = 10000000;

        public bool IgniteIsStaticIpFinder { get; set; } = true;

        public ICollection<string> IgniteClusterEndpoints { get; set; } = new List<string>();

        public bool IgniteForceServerMode { get; set; } = false;

        public bool ContainsCacheName(string cacheName)
        {
            if (string.IsNullOrEmpty(cacheName))
            {
                throw new ArgumentNullException("CacheName");
            }

            if (CacheConfigurations != null)
            {
                IEnumerable<CacheManagersEntry> query = from t in CacheConfigurations
                                                        where t.Name.Equals(cacheName.Replace('_', '.'))
                                                        select t;

                return query.FirstOrDefault() != null;
            }

            return false;
        }

        public bool IsUseDataCacheClientConfiguration(string cacheName)
        {
            if (string.IsNullOrEmpty(cacheName))
            {
                throw new ArgumentNullException("CacheName");
            }

            if (CacheConfigurations != null)
            {
                IEnumerable<CacheManagersEntry> query = from t in CacheConfigurations
                                                        where t.Name.Equals(cacheName.Replace('_', '.')) && t.UseDataCacheClientConfigruation
                                                        select t;

                return query.FirstOrDefault() != null;
            }

            return false;
        }

        #endregion        
    }
}
