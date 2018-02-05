using CommonX.Utilities;
using System;
using System.Collections.Generic;
using System.Configuration;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using System.Linq;
using System.Text;

namespace CommonX.Cache.Configuration
{
    [Serializable]
    public class CacheConfiguration : ConfigurationSection, ICacheConfiguration
    {
        //public const string SectionName = "CacheConfiguration";
        private const string ConstantRefreshMinutesProperty = "refreshMinutes";

        private const string ConstantEntLibConfigurationSectionName = "CacheConfiguration";

        /// <summary>
        ///     Name of cacheManagersCollection from the configuration file that populates the
        ///     <see cref="CacheManagersCollection" /> property of the <see cref="CacheConfiguration" /> class.
        /// </summary>
        private const string ConstantCacheManagersCollectionProperty = "cacheManagers";

        /// <summary>
        ///     Name of setting from the configuration file that populates the
        ///     <see cref="SettingsCollection" /> property of the <see cref="CacheConfiguration" /> class.
        /// </summary>
        private const string ConstantSettingsCollectionProperty = "settings";

        private static readonly object syncRoot = new object();

        private static DateTime lastRefreshDateTime = DateTime.UtcNow;

        private static int lastReadRefreshMinutes = 1;

        /// <summary>
        ///     Property that must be used to an instance of this class.
        /// </summary>
        /// <returns>
        ///     CacheConfiguration
        /// </returns>
        [SuppressMessage("Microsoft.Design", "CA1065")]
        public ICacheConfiguration Instance
        {
            get
            {
                try
                {
                    if (IsTimeToRefresh)
                    {
                        lock (syncRoot)
                        {
                            if (IsTimeToRefresh)
                            {
                                ConfigurationManager.RefreshSection(ConstantEntLibConfigurationSectionName);
                                lastRefreshDateTime = DateTime.UtcNow;
                            }
                        }
                    }

                    var returnValue = (CacheConfiguration)
                        ConfigurationManager.GetSection(ConstantEntLibConfigurationSectionName);

                    if (returnValue != null)
                    {
                        lastReadRefreshMinutes = returnValue.RefreshMinutes;
                        ;
                    }
                    else
                    {
                        returnValue = new CacheConfiguration();
                        //ExceptionHandler.HandleException(new UnexpectedCacheException(
                        //    "Creating instance of CacheConfiguration failed.  The call to CacheConfiguration.GetSection returned a null.  " +
                        //    "Check the entries in the configuration file to make sure they are present and in the correct case."),
                        //    ExceptionHandlingPolicy.LogOnly);
                    }

                    return returnValue;
                }
                catch (Exception sysException)
                {
                    //sysException.Data[ExceptionUtils.BuildFullDataName("ConstantEntLibConfigurationSectionName")] =
                    //    ConstantEntLibConfigurationSectionName;
                    throw;
                }
            }
        }

        /// <summary>
        ///     Determines whether a new instance of this class is needed.  Either
        ///     because it hasn't yet been instantiated, or because enough time
        ///     has expired so a refresh is warranted.
        /// </summary>
        private static bool IsTimeToRefresh
        {
            get
            {
                return ConfigurationUtils.IsTimeToRefresh(
                    lastRefreshDateTime, lastReadRefreshMinutes);
            }
        }

        public string CacheKeyPrefix
        {
            get { return Settings.Get("CacheKeyPrefix")?.Value; }
            set { }
        }

        public bool UseRedisCache
        {
            get { return Convert.ToBoolean(Settings.Get("UseRedisCache")?.Value); }
            set { }
        }

        public bool UseFullCache
        {
            get { return Convert.ToBoolean(Settings.Get("UseFullCache")?.Value); }
            set { }
        }

        public int FatalSuspendSeconds
        {
            get
            {
                var nFatalSuspendSeconds = 300;
                if (Settings.Contains("FatalSuspendSeconds"))
                {
                    nFatalSuspendSeconds = Convert.ToInt32(Settings.Get("FatalSuspendSeconds").Value);
                }
                return nFatalSuspendSeconds;
            }
            set { }
        }

        public int MaxFatalTimesBeforeSuspend
        {
            get
            {
                var nMaxFatalTimesBeforeSuspend = 3;
                if (Settings.Contains("MaxFatalTimesBeforeSuspend"))
                {
                    nMaxFatalTimesBeforeSuspend = Convert.ToInt32(Settings.Get("MaxFatalTimesBeforeSuspend").Value);
                }
                return nMaxFatalTimesBeforeSuspend;
            }
            set { }
        }

        public bool IsUseDataCacheClientConfigurationForAll
        {
            get
            {
                var bIsUseDataCacheClientConfigurationForAll = false;
                if (Settings.Contains("IsUseDataCacheClientConfigurationForAll"))
                {
                    bIsUseDataCacheClientConfigurationForAll =
                        Convert.ToBoolean(Settings.Get("IsUseDataCacheClientConfigurationForAll").Value);
                }
                return bIsUseDataCacheClientConfigurationForAll;
            }
            set { }
        }

        public List<CacheManagersEntry> CacheConfigurations
        {
            get
            {
                var listResult = new List<CacheManagersEntry>();
                foreach (object item in CacheManagers)
                {
                    listResult.Add((item as CacheManagersEntry));
                }
                return listResult;
            }
            set { }
        }

        public string RegionName { get; set; }

        public int? RequestTimeout
        {
            get
            {
                int? nRequestTimeout = 0;
                if (Settings.Contains("RequestTimeout"))
                {
                    nRequestTimeout = Convert.ToInt32(Settings.Get("RequestTimeout").Value);
                }
                return nRequestTimeout;
            }
            set { }
        }

        public int? ChannelOpenTimeout
        {
            get
            {
                int? nChannelOpenTimeout = 0;
                if (Settings.Contains("ChannelOpenTimeout"))
                {
                    nChannelOpenTimeout = Convert.ToInt32(Settings.Get("ChannelOpenTimeout").Value);
                }
                return nChannelOpenTimeout;
            }
            set { }
        }

        /// <summary>
        ///     Returns a collection of <see cref="CacheManagersEntry" /> objects
        ///     that contain the exact data read from the configuration file.
        /// </summary>
        [ConfigurationProperty(ConstantCacheManagersCollectionProperty)]
        public virtual CacheManagersCollection CacheManagers
        {
            get { return (CacheManagersCollection)base[ConstantCacheManagersCollectionProperty]; }
        }

        /// <summary>
        ///     Returns a collection of <see cref="SettingsEntry" /> objects
        ///     that contain the exact data read from the configuration file.
        /// </summary>
        [ConfigurationProperty(ConstantSettingsCollectionProperty)]
        public virtual SettingsCollection Settings
        {
            get { return (SettingsCollection)base[ConstantSettingsCollectionProperty]; }
        }

        [ConfigurationProperty(ConstantRefreshMinutesProperty, DefaultValue = 60)]
        public int RefreshMinutes
        {
            get { return Convert.ToInt32(this[ConstantRefreshMinutesProperty], CultureInfo.InvariantCulture); }
            set { this[ConstantRefreshMinutesProperty] = value; }
        }

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

        #region TransportProperties

        public int TransportProperties_MaxBufferSize
        {
            get { return Convert.ToInt32(Settings.Get("maxBufferSize").Value); }
            set { }
        }

        public int TransportProperties_ConnectionBufferSize
        {
            get { return Convert.ToInt32(Settings.Get("connectionBufferSize").Value); }
            set { }
        }

        public long TransportProperties_MaxBufferPoolSize
        {
            get { return Convert.ToInt64(Settings.Get("maxBufferPoolSize").Value); }
            set { }
        }

        public long TransportProperties_MaxOutputDelay
        {
            get { return Convert.ToInt64(Settings.Get("maxOutputDelay").Value); }
            set { }
        }

        public long TransportProperties_ChannelInitializationTimeout
        {
            get { return Convert.ToInt64(Settings.Get("channelInitializationTimeout").Value); }
            set { }
        }

        public long TransportProperties_ReceiveTimeout
        {
            get { return Convert.ToInt64(Settings.Get("receiveTimeout").Value); }
            set { }
        }

        #endregion

        #region SecurityProperties

        public string SecurityProperties_SecurityMode
        {
            get { return Settings.Get("SecurityMode").Value; }
            set { }
        }

        public string SecurityProperties_ProtectionLevel
        {
            get { return Settings.Get("ProtectionLevel").Value; }
            set { }
        }

        #endregion

        #region IgniteProperty


        public string IgniteGridName
        {
            get { return Settings.Get("GridName")?.Value; }
            set { }
        }

        public bool IgniteForceServerMode
        {
            get
            {
                if (Settings.Get("ForceServerMode") != null)
                {
                    return bool.Parse(Settings.Get("ForceServerMode").Value);
                }
                else
                {
                    return false;
                }
            }
        }
        public bool IgniteIsStaticIpFinder
        {
            get { return bool.Parse(Settings.Get("IsStaticIpFinder").Value); }
            set { }
        }

        public ICollection<string> IgniteClusterEndpoints
        {
            get { return Settings.Get("ClusterEndpoints").Value.Split(','); }
            set { }
        }

        public long IgniteOffHeapMaxMemory
        {
            get
            {
                long re = 0;
                if (Settings.Contains("OffHeapMaxMemory"))
                {
                    long.TryParse(Settings.Get("OffHeapMaxMemory").Value, out re);
                }

                return re;
            }
            set { }
        }
        public bool IgniteEnableSwap
        {
            get
            {
                //bool re = false;
                //if (Settings.Contains("EnableSwap"))
                //{
                //    bool.TryParse(Settings.Get("EnableSwap").Value, out re);
                //}
                //return re;
                /*Hanson 升级ignite 2.0，无交换分区概念，默认为false*/
                return false;
            }
            set { }
        }
        public int IgniteMemoryMode
        {
            get
            {
                //return Convert.ToInt32(CacheMemoryMode.OffheapTiered);
                //try
                //{
                //    return Convert.ToInt32(Settings.Get("MemoryMode").Value);
                //    //re = (CacheMemoryMode)Enum.Parse(typeof(CacheMemoryMode), Settings.Get("MemoryMode").Value);
                //}
                //catch {
                //    return Convert.ToInt32(CacheMemoryMode.OffheapTiered);
                //}
                return 1;
            }
            set { }
        }
        public int IgniteSqlOnheapRowCacheSize
        {
            get
            {
                int re = 10000000;
                if (Settings.Contains("SqlOnheapRowCacheSize"))
                {
                    int.TryParse(Settings.Get("SqlOnheapRowCacheSize").Value, out re);
                }
                return re;
            }
            set { }
        }
        #endregion
    }
}
