//using Apache.Ignite.Core;
//using Apache.Ignite.Core.Binary;
//using Apache.Ignite.Core.Cache.Configuration;
//using Apache.Ignite.Core.Cache.Store;
//using Apache.Ignite.Core.Common;
//using Apache.Ignite.Core.Discovery.Tcp;
//using Apache.Ignite.Core.Discovery.Tcp.Static;
//using Apache.Ignite.Core.Log;
//using ECommon.Components;
//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Configuration;
//using System.IO;
//using System.Linq;
//using System.Reflection;

//namespace ECommon.Cache.Ignite
//{
//    /// <summary>
//    ///     configuration class Ignite extensions.
//    /// </summary>
//    public static class ConfigurationExtensions
//    {

//        /// <summary>
//        /// Ignite通用缓存
//        /// </summary>
//        /// <param name="configuration"></param>
//        /// <param name="clientMode">客户端模式启动</param>
//        /// <param name="isSelectProd">是否定制的商品选择缓存</param>
//        /// <param name="cacheConfigType">缓存配置读取方式：1-CacheConfiguration.config文件    2-从数据库中读取</param>
//        /// <returns></returns>
//        public static Configurations.Configuration UseIgniteCache(this Configurations.Configuration configuration,
//            ICacheConfiguration cacheConfiguration, bool clientMode = true, bool isSelectProd = false)
//        {
//            //var cacheConfiguration = Configuration.CacheConfiguration.Instance;


//            //ICacheConfiguration cacheConfiguration = null;

//            ////根据配置Resolve缓存配置[读取数据库 | 读取config文件]
//            //if (Configurations.Configuration.Instance.Setting.UseDbCacheConfiguration && cacheConfigType == 2)
//            //    ObjectContainer.Current.Register<ICacheConfiguration, JZTERP.Frameworks.Common.Cache.Impl.CacheConfiguration>();
//            //using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//            //    cacheConfiguration = scope.Resolve<ICacheConfiguration>();

//            //只读
//            NameValueCollection appSettings = ConfigurationManager.AppSettings;
//            var defaultAppSettings = new NameValueCollection();
//            if (clientMode)
//                defaultAppSettings = new NameValueCollection
//                {
//                    {"xms-g", "2.5"},
//                    {"xmx-g", "2.5"},
//                    {"jvmOptions-1", "-XX:+UseParNewGC"},
//                    {"jvmOptions-2", "-XX:+UseConcMarkSweepGC"},
//                    {"jvmOptions-3", "-XX:+UseTLAB"},
//                    {"jvmOptions-4", "-XX:NewSize=128m"},
//                    {"jvmOptions-5", "-XX:MaxNewSize=128m"},
//                    {"jvmOptions-6", "-XX:MaxTenuringThreshold=0"},
//                    {"jvmOptions-7", "-XX:SurvivorRatio=1024"},
//                    {"jvmOptions-8", "-XX:+UseCMSInitiatingOccupancyOnly"},
//                    {"jvmOptions-9", "-XX:CMSInitiatingOccupancyFraction=60"},
//                    {"jvmOptions-10", "-Djava.net.preferIPv4Stack=true"}
//                };
//            foreach (var key in appSettings.AllKeys)
//                if (null == defaultAppSettings[key]) defaultAppSettings.Add(key, appSettings[key]);
//                else defaultAppSettings[key] = appSettings[key];

//            var igniteCfg = (NameValueCollection)ConfigurationManager.GetSection("IgniteConfig");
//            if (igniteCfg != null)
//            {
//                foreach (var key in igniteCfg.AllKeys)
//                    if (null == defaultAppSettings[key]) defaultAppSettings.Add(key, igniteCfg[key]);
//                    else defaultAppSettings[key] = igniteCfg[key];
//            }

//            //ObjectContainer.Current.RegisterGeneric(typeof(ICache<,>), typeof(IgniteCache<,>));
//            //ObjectContainer.Current.RegisterGeneric(typeof(IStore<>), typeof(Store<>));
//            ObjectContainer.Current.RegisterGeneric(typeof(IFactory<>), typeof(StoreFactory<>));
//            //ObjectContainer.Current.RegisterGeneric(typeof(IFactory<>).GetType().MakeGenericType(typeof(IStore<>).GetType()), typeof(StoreFactory<>));

//            ObjectContainer.Current.RegisterGeneric(typeof(ICacheLoader<,>), typeof(CacheLoaderBase<>), LifeStyle.Transient);
//            ObjectContainer.Current.RegisterGeneric(typeof(ICacheLoader<,>), typeof(CacheLoader<>), LifeStyle.Transient);
//            ObjectContainer.Current.RegisterGeneric(typeof(IDbChangeProcessor<,>), typeof(DbChangeProcessor<>), LifeStyle.Transient);

//            if (cacheConfiguration.Instance.UseFullCache)
//            {
//                ObjectContainer.Current.Register<ICacheFactory, IgniteCacheFactory>();
//            }

//            #region 初始化IgniteConfiguration

//            var igniteConfig = new IgniteConfiguration();
//            igniteConfig.ClientMode = clientMode;
//            //if (!string.IsNullOrEmpty(cacheConfiguration.Instance.IgniteGridName))
//            //    igniteConfig.GridName = cacheConfiguration.Instance.IgniteGridName;
//            //防止IIS重启的时候Ignite实例回收过程还没完成报实例重复错误
//            igniteConfig.GridName = Guid.NewGuid().ToString();

//            if (defaultAppSettings["igniteHome"] != null)
//                igniteConfig.IgniteHome = defaultAppSettings["igniteHome"];
//            if (defaultAppSettings["jvmDllPath"] != null)
//                igniteConfig.JvmDllPath = defaultAppSettings["jvmDllPath"];

//            string igniteHomePath = IgniteHome.Resolve(igniteConfig);
//            if (!string.IsNullOrEmpty(igniteHomePath))
//            {
//                var igniteHomeDir = new DirectoryInfo(igniteHomePath);
//                var defaultSpringConfigPath = igniteHomeDir.GetFiles("config/default-config.xml").FirstOrDefault();
//                if (defaultSpringConfigPath != null)
//                {
//                    igniteConfig.SpringConfigUrl = defaultSpringConfigPath.FullName;
//                }
//            }

//            #endregion

//            #region 设置java jvm

//            if (defaultAppSettings["xms-g"] != null)
//                igniteConfig.JvmInitialMemoryMb = (int)(float.Parse(defaultAppSettings["xms-g"]) * 1024);
//            if (defaultAppSettings["xmx-g"] != null)
//                igniteConfig.JvmMaxMemoryMb = (int)(float.Parse(defaultAppSettings["xmx-g"]) * 1024);
//            else
//                igniteConfig.JvmMaxMemoryMb = igniteConfig.JvmInitialMemoryMb;

//            var jvmOptions = new List<string>();
//            foreach (var key in defaultAppSettings.AllKeys)
//                if (key.StartsWith("jvmOptions"))
//                    jvmOptions.Add(defaultAppSettings[key]);

//            //根据官方建议，设置GC的size
//            igniteConfig.JvmOptions = jvmOptions;

//            #endregion

//            #region 设置BinaryConfiguration 和 CacheConfiguration

//            ICollection<CacheConfiguration> cacheConfig = new List<CacheConfiguration>();
//            //var binaryConfig = new BinaryConfiguration
//            //{
//            //    TypeConfigurations = new List<BinaryTypeConfiguration>()
//            //};
//            foreach (var cacheEntry in cacheConfiguration.Instance.CacheConfigurations.Where(c => c.AllCache))
//            {
//                var typeCache = Type.GetType(cacheEntry.Name);
//                var keyType = string.IsNullOrEmpty(cacheEntry.Key) ? typeof(long) : Type.GetType(cacheEntry.Key);

//                if (typeCache == null) continue;
//                //binaryConfig.TypeConfigurations.Add(new BinaryTypeConfiguration(type)
//                //{
//                //    Serializer = new BinaryReflectiveSerializer { RawMode = false }
//                //});

//                var cacheItem = new CacheConfiguration();
//                cacheConfig.Add(cacheItem);
//                cacheItem.Backups = 0;
//                cacheItem.Name = typeCache.Name;
//                //if (!string.IsNullOrEmpty(cacheEntry.CacheMode))
//                //    cacheItem.CacheMode = (CacheMode) Enum.Parse(typeof(CacheMode), cacheEntry.CacheMode);
//                //else
//                //{
//                //    cacheItem.CacheMode = CacheMode.Replicated;
//                //}

//                //if (cacheItem.CacheMode == CacheMode.Partitioned)
//                //{
//                //    //affinity CacheStoreAffinityFunction
//                //    var func = new CacheStoreAffinityFunction();
//                //    func.NodeCount = string.IsNullOrEmpty(cacheEntry.NodeCount) ? 1 : int.Parse(cacheEntry.NodeCount);
//                //    cacheItem.AffinityFunction = new CacheStoreAffinityFunction();
//                //}
//                cacheItem.KeepBinaryInStore = true;

//                /* 升级ignite 2.0 删除*/
//                //cacheItem.OffHeapMaxMemory = 0;//cacheConfiguration.Instance.IgniteOffHeapMaxMemory; 
//                //cacheItem.EnableSwap = false;//cacheConfiguration.Instance.IgniteEnableSwap;
//                //cacheItem.MemoryMode = CacheMemoryMode.OffheapTiered; //(CacheMemoryMode)cacheConfiguration.Instance.IgniteMemoryMode;
//                //cacheItem.SqlOnheapRowCacheSize = 10000000;//cacheConfiguration.Instance.IgniteSqlOnheapRowCacheSize;
//                /* 升级ignite 2.0 删除*/

//                //CacheFactory
//                //TODO: 跨平台。以下代码导致互操作性错误，在load完成后抛出异常。如果要跨平台必须使用直接加载的方式
//                if (isSelectProd)
//                {
//                    cacheItem.ReadThrough = true;
//                    var factory = CacheStoreConfiguration.Instance()
//                        .CacheFactory;
//                    var factoryType = Type.GetType(factory);

//                    var genericType = factoryType.MakeGenericType(keyType, typeCache);

//                    cacheItem.CacheStoreFactory = (IFactory<ICacheStore>)Activator.CreateInstance(genericType);
//                }
//                var entities = new List<QueryEntity>();
//                cacheItem.QueryEntities = entities;
//                var entity = new QueryEntity();
//                entities.Add(entity);
//                entity.KeyType = string.IsNullOrEmpty(cacheEntry.Key) ? null : Type.GetType(cacheEntry.Key);
//                entity.KeyTypeName = entity.KeyType == null ? typeof(long).FullName : entity.KeyType.FullName;
//                entity.ValueType = Type.GetType(cacheEntry.Name);
//                entity.ValueTypeName = entity.ValueType.FullName;
//                //query fields
//                if (entity.Fields == null)
//                { entity.Fields = new List<QueryField>(); }
//                //只有有索引的才有field
//                //query indexs
//                if (!string.IsNullOrEmpty(cacheEntry.Indexs))
//                {
//                    entity.Indexes = new List<QueryIndex>();

//                    var idx = cacheEntry.Indexs.Split(',').Select(c => c.Trim()).ToArray();
//                    //var realIdx = new List<string>();
//                    var properties = typeCache.GetProperties().ToDictionary(c => c.Name.ToLower(), d => d);
//                    foreach (var i in idx)
//                    {
//                        if (properties.ContainsKey(i.ToLower()))
//                        {
//                            var it = properties[i.ToLower()];
//                            if (JavaTypeDict.IsExist(it.PropertyType))
//                                entity.Fields.Add(new QueryField(it.Name, JavaTypeDict.GetJavaType(it.PropertyType)));
//                            else
//                                entity.Fields.Add(new QueryField(it.Name, it.PropertyType));

//                            entity.Indexes.Add(new QueryIndex(it.Name));
//                            //realIdx.Add(it.Name);
//                        }
//                    }
//                    //entity.Indexes.Add(new QueryIndex(realIdx.ToArray()));
//                }
//                else
//                {
//                    var properties = typeCache.GetProperties();
//                    foreach (var it in properties)
//                    {
//                        if (JavaTypeDict.IsExist(it.PropertyType))
//                            entity.Fields.Add(new QueryField(it.Name, JavaTypeDict.GetJavaType(it.PropertyType)));
//                        else
//                            entity.Fields.Add(new QueryField(it.Name, it.PropertyType));
//                    }
//                }
//            }
//            //igniteConfig.BinaryConfiguration = binaryConfig;
//            igniteConfig.CacheConfiguration = cacheConfig;

//            #endregion

//            #region 设置queuelimit
//            var tcpcfg = new Apache.Ignite.Core.Communication.Tcp.TcpCommunicationSpi();
//            tcpcfg.SlowClientQueueLimit = 20000;
//            igniteConfig.CommunicationSpi = tcpcfg;
//            #endregion

//            #region 设置 DiscoverySPI

//            var spi = new TcpDiscoverySpi();
//            igniteConfig.DiscoverySpi = spi;
//            //hard code for the configuration
//            if (clientMode)
//            {
//                spi.ForceServerMode = cacheConfiguration.Instance.IgniteForceServerMode;
//            }
//            spi.JoinTimeout = TimeSpan.FromMilliseconds(0);
//            //这个至少需要等于NetworkTimeout以便允许重连发生
//            spi.AckTimeout = TimeSpan.FromMilliseconds(60000);
//            spi.SocketTimeout = TimeSpan.FromMilliseconds(2000);
//            spi.NetworkTimeout = TimeSpan.FromMilliseconds(60000);
//            spi.ReconnectCount = 500;
//            //升級到ignite 2.0后刪除
//            //spi.HeartbeatFrequency = TimeSpan.FromMilliseconds(20000);
//            //spi.MaxMissedHeartbeats = 5;
//            //if (Configuration.CacheConfiguration.Instance.IgniteIsStaticIpFinder)
//            //if (cacheConfiguration.Instance.IgniteIsStaticIpFinder)
//            //{
//            var sFinder = new TcpDiscoveryStaticIpFinder();
//            spi.IpFinder = sFinder;
//            //sFinder.Endpoints = Configuration.CacheConfiguration.Instance.IgniteClusterEndpoints;
//            sFinder.Endpoints = cacheConfiguration.Instance.IgniteClusterEndpoints;
//            //}
//            //else
//            //{
//            //    var mfinder = new TcpDiscoveryMulticastIpFinder();
//            //    spi.IpFinder = mfinder;
//            //    //mfinder.Endpoints = Configuration.CacheConfiguration.Instance.IgniteClusterEndpoints;
//            //    mfinder.Endpoints = cacheConfiguration.Instance.IgniteClusterEndpoints;
//            //}
//            #endregion

//            #region 启动Ignite实例
//            var ignite = Ignition.Start(igniteConfig);
//            ObjectContainer.Current.RegisterInstance<IIgnite, IIgnite>(ignite, LifeStyle.Singleton);
//            AppDomain.CurrentDomain.DomainUnload += (o, e) =>
//            {
//                Ignition.StopAll(true);
//            };
//            RegisterCacheRelationship(ignite, cacheConfiguration);
//            #endregion

//            return configuration;
//        }

//        public static Configurations.Configuration UseDbChangeService(this Configurations.Configuration configuration)
//        {
//            using (var scope = ObjectContainer.BeginLifetimeScope())
//            {
//                var csc = CacheStoreConfiguration.Instance();
//                var tableNames = csc.GetTableNamesOfDbChangeProcessors();
//                ObjectContainer.Current.Register<IDbNotificationService, DbNotificationService>(LifeStyle.Singleton);
//                var igniteCfg = (NameValueCollection)ConfigurationManager.GetSection("IgniteConfig");
//                var dbNotificationService = scope.Resolve<IDbNotificationService>();
//                dbNotificationService.Start(igniteCfg["DbNotificationModule"] + "", tableNames, igniteCfg["DbNotificationHost"] + "", igniteCfg["DbNotificationUser"] + "", igniteCfg["DbNotificationPassword"] + "");
//                return configuration;
//            }
//        }

//        public static void RegisterCacheRelationship(IIgnite ignite, ICacheConfiguration cacheConfiguration)
//        {
//            using (var scope = ObjectContainer.BeginLifetimeScope())
//            {
//                var dbContext = scope.Resolve<DbContext>();
//                var config = ignite.GetConfiguration();
//                //Key: CacheName, Value: Cache中包含的实体类型fullName集合
//                var loading = new Dictionary<string, Type>();
//                foreach (var c in config.CacheConfiguration)
//                {
//                    if (c.QueryEntities == null) continue;
//                    if (!loading.ContainsKey(c.Name))
//                    {
//                        var entityFullNames = new List<string>();
//                        foreach (var qe in c.QueryEntities)
//                        {
//                            var fullName = qe.ValueTypeName;
//                            if (!entityFullNames.Contains(fullName)) entityFullNames.Add(fullName);
//                        }
//                        var entityFullName = cacheConfiguration.Instance.CacheConfigurations.FirstOrDefault(d => d.Name.Split(',')[0] == (entityFullNames.FirstOrDefault()))?.Name;
//                        loading.Add(c.Name, Type.GetType(entityFullName));
//                    }
//                }
//                foreach (var c in loading.Keys)
//                {
//                    var storeConfig = CacheStoreConfiguration.Instance()
//                        .LoaderConfig(c);
//                    if (storeConfig == null)
//                    {
//                        RelationshipManager.Register(dbContext, loading[c]);
//                    }

//                }
//            }
//        }

//        #region utilities


//        #endregion
//    }

//    /// <summary>
//    /// IgniteHome resolver.
//    /// </summary>
//    internal static class IgniteHome
//    {
//        /** Environment variable: IGNITE_HOME. */
//        internal const string EnvIgniteHome = "IGNITE_HOME";

//        /// <summary>
//        /// Calculate Ignite home.
//        /// </summary>
//        /// <param name="cfg">Configuration.</param>
//        /// <param name="log">The log.</param>
//        public static string Resolve(IgniteConfiguration cfg, ILogger log = null)
//        {
//            var home = cfg == null ? null : cfg.IgniteHome;

//            if (string.IsNullOrWhiteSpace(home))
//            {
//                home = Environment.GetEnvironmentVariable(EnvIgniteHome);

//                if (log != null)
//                    log.Debug("IgniteHome retrieved from {0} environment variable: '{1}'", EnvIgniteHome, home);
//            }
//            else if (!IsIgniteHome(new DirectoryInfo(home)))
//                throw new IgniteException(string.Format("IgniteConfiguration.IgniteHome is not valid: '{0}'", home));

//            if (string.IsNullOrWhiteSpace(home))
//                home = Resolve(log);
//            else if (!IsIgniteHome(new DirectoryInfo(home)))
//                throw new IgniteException(string.Format("{0} is not valid: '{1}'", EnvIgniteHome, home));

//            if (log != null)
//                log.Debug("IgniteHome resolved to '{0}'", home);

//            return home;
//        }

//        /// <summary>
//        /// Automatically resolve Ignite home directory.
//        /// </summary>
//        /// <param name="log">The log.</param>
//        /// <returns>
//        /// Ignite home directory.
//        /// </returns>
//        private static string Resolve(ILogger log)
//        {
//            var probeDirs = new[]
//            {
//                Path.GetDirectoryName(Assembly.GetExecutingAssembly().Location),
//                Directory.GetCurrentDirectory()
//            };

//            if (log != null)
//                log.Debug("Attempting to resolve IgniteHome in the assembly directory " +
//                          "'{0}' and current directory '{1}'...", probeDirs[0], probeDirs[1]);


//            foreach (var probeDir in probeDirs.Where(x => !string.IsNullOrEmpty(x)))
//            {
//                if (log != null)
//                    log.Debug("Probing IgniteHome in '{0}'...", probeDir);

//                var dir = new DirectoryInfo(probeDir);

//                while (dir != null)
//                {
//                    if (IsIgniteHome(dir))
//                        return dir.FullName;

//                    dir = dir.Parent;
//                }
//            }

//            return null;
//        }

//        /// <summary>
//        /// Determines whether specified dir looks like a Ignite home.
//        /// </summary>
//        /// <param name="dir">Directory.</param>
//        /// <returns>Value indicating whether specified dir looks like a Ignite home.</returns>
//        private static bool IsIgniteHome(DirectoryInfo dir)
//        {
//            return dir.Exists &&
//                   (dir.EnumerateDirectories().Count(x => x.Name == "examples" || x.Name == "bin") == 2 &&
//                    dir.EnumerateDirectories().Count(x => x.Name == "modules" || x.Name == "platforms") == 1)
//                   || // NuGet home
//                   (dir.EnumerateDirectories().Any(x => x.Name == "Libs") &&
//                    (dir.EnumerateFiles("Apache.Ignite.Core.dll").Any() ||
//                     dir.EnumerateFiles("Apache.Ignite.*.nupkg").Any()));
//        }
//    }
//}