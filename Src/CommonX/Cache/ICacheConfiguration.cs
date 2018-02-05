using CommonX.Cache.Configuration;
using System;
using System.Collections.Generic;
using System.Text;

namespace CommonX.Cache
{
    public interface ICacheConfiguration
    {
        ICacheConfiguration Instance { get; }

        string CacheKeyPrefix { get; set; }

        int TransportProperties_MaxBufferSize { get; set; }

        int RefreshMinutes { get; set; }

        bool UseFullCache { get; set; }

        bool UseRedisCache { get; set; }

        string IgniteGridName { get; set; }

        long IgniteOffHeapMaxMemory { get; set; }

        bool IgniteEnableSwap { get; set; }

        int IgniteMemoryMode { get; set; }

        int IgniteSqlOnheapRowCacheSize { get; set; }

        bool IgniteIsStaticIpFinder { get; set; }

        ICollection<string> IgniteClusterEndpoints { get; set; }

        bool IgniteForceServerMode { get; }

        List<CacheManagersEntry> CacheConfigurations { get; set; }

        bool ContainsCacheName(string cacheName);

        bool IsUseDataCacheClientConfiguration(string cacheName);

    }
}
