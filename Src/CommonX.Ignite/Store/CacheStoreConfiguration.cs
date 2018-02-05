//using Apache.Ignite.Core;

//using System;
//using System.Collections.Generic;
//using System.Configuration;
//using System.Linq;
//using System.Reflection;
//using System.Text;
//using System.Threading.Tasks;
//using System.Xml;
//using ECommon.Components;
//using ECommon.Logging;
//using ECommon.Cache.Ignite.CacheLoaders;

//namespace ECommon.Cache.Ignite.Store
//{
//    public class CacheStoreConfiguration
//    {
//        static CacheStoreConfiguration()
//        {
//        }
//        private CacheStoreConfiguration()
//        {
//            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//            {
//                LoggerFactory = scope.Resolve<ILoggerFactory>();
//                //schemaName = scope.Resolve<DbContext>().Database.Connection.ConnectionString.Split(';')[0].Split('=')[1].ToUpper();
//            }
//        }
//        public ILoggerFactory LoggerFactory;
//        private static readonly object locker = new object();
//        private static CacheStoreConfiguration _instance;
//        private string schemaName = string.Empty;
//        string filePath;

//        System.IO.FileSystemWatcher fileWatcher;
        
//        private void OnConfigChanged(object sender, System.IO.FileSystemEventArgs args)
//        {
//        }

//        private List<IDbChangeProcessor> DbChangeProcessors = new List<IDbChangeProcessor>();
//        private Dictionary<string, string> branchesAndHermesIds = new Dictionary<string, string>();
//        public string GetHermesId(string branchId)
//        {
//            if (branchesAndHermesIds.ContainsKey(branchId)) return branchesAndHermesIds[branchId];
//            return null;
//        }
//        public List<String> GetBranchIds()
//        {
//            return branchesAndHermesIds.Keys.ToList();
//        }
//        private void BuildUp()
//        {
//            this.Loaders = new Dictionary<string, CacheLoaderConfiguration>();
//            System.Xml.XmlDocument doc = new XmlDocument();
//            doc.Load(this.filePath);

//            CacheFactory = doc.DocumentElement.Attributes["Factory"].Value;
//            var appsettings_branchIds = ConfigurationManager.AppSettings["Branch"].Split(',').ToList();
//            foreach(var bid in appsettings_branchIds)
//            {
//                branchesAndHermesIds.Add(bid, Configurations.Setting.CONST_CONN_NAME);
//            }

//            var branchesAndHermesIds_configFileInfo = new System.IO.FileInfo(this.filePath).Directory.GetFiles("hermes.config").FirstOrDefault();
//            if (branchesAndHermesIds_configFileInfo != null)
//            {
//                System.Xml.XmlDocument branchesXml = new System.Xml.XmlDocument();
//                branchesXml.Load(branchesAndHermesIds_configFileInfo.FullName);
//                foreach (XmlNode branch in branchesXml.DocumentElement.SelectNodes("branches/add"))
//                {
//                    string branchId = branch.Attributes["branchId"].Value;
//                    string hermesId = branch.Attributes["hermesId"] == null ? null : branch.Attributes["hermesId"].Value;
//                    if (!branchesAndHermesIds.ContainsKey(branchId))
//                        branchesAndHermesIds.Add(branchId, hermesId);
//                    else
//                        branchesAndHermesIds[branchId] = hermesId;
//                }
//            }
//            foreach (XmlNode store in doc.DocumentElement.ChildNodes)
//            {
//                HandleStoreNode(store);
//            }
//            //以下逻辑不能再CacheLoaderConfiguration中使用，会造成循环调用堆栈溢出错误
//            foreach(var loader in this.Loaders.Values)
//            {
//                foreach(var l in loader.GetAll())
//                {
//                    l.Branches = this.GetBranchIds();
//                }
//            }
//        }
//        private void HandleStoreNode(XmlNode store)
//        {
//            if (store.NodeType != XmlNodeType.Element) return;
//            var cache = store.Attributes["cache"].Value;
//            if (this.Loaders.ContainsKey(cache)) this.Loaders.Remove(cache);
//            var loaderConfig = new CacheLoaderConfiguration();
//            this.Loaders.Add(cache, loaderConfig);
//            foreach (XmlElement node in store.SelectNodes("loaders/add"))
//            {
//                var type = node.Attributes["type"].Value;
//                var isAsync = bool.Parse(node.Attributes["IsAsyn"].Value);
//                loaderConfig.AddLoader(type, isAsync);
//            }
//            //获取dbSync节点
//            var dbSyncNode = store.SelectSingleNode("dbSync");
//            //schema取数据库连接的
//            //var schemaName = dbSyncNode != null ? dbSyncNode.Attributes["schema"].InnerText : string.Empty;
//            foreach (XmlElement receiver in store.SelectNodes("dbSync/add"))
//            {
//                var entityType = receiver.SelectSingleNode("entity").InnerText;
//                var handlerType = receiver.SelectSingleNode("handler").InnerText;
//                var onDescList = new List<OnDesc>();
//                foreach (XmlElement on in receiver.SelectNodes("on"))
//                {
//                    var tableName = on.GetAttribute("tableName");
//                    if (!string.IsNullOrEmpty(schemaName))
//                    {
//                        tableName = schemaName + "." + tableName;
//                    }
//                    var update = on.GetAttribute("update");
//                    var delete = on.GetAttribute("delete");
//                    var insert = on.GetAttribute("insert");
//                    var keysString = on.GetAttribute("primaryKeys");
//                    var keys = string.IsNullOrEmpty(keysString) ? new string[] { } : keysString.Split(',');

//                    onDescList.Add(new OnDesc
//                    {
//                        Table = tableName,
//                        Update = update != null && update.ToLower() == "true",
//                        Delete = delete != null && delete.ToLower() == "true",
//                        Insert = insert != null && insert.ToLower() == "true",
//                        PrimaryKeys = keys
//                    });
//                }
//                var dbStreamType = Type.GetType(handlerType);
//                foreach (OnDesc on in onDescList)
//                {
//                    if (!CheckTypeSerializable(dbStreamType))
//                        throw new ConfigurationErrorsException("DbStreamer must be serializable:" + dbStreamType.Name);
//                    IDbChangeProcessor dbs = dbStreamType.GetConstructor(new Type[] { }).Invoke(new object[] { }) as IDbChangeProcessor;
//                    dbs.CacheName = cache;
//                    dbs.EntityType = entityType;
//                    dbs.Table = on.Table;
//                    dbs.Update = on.Update;
//                    dbs.Delete = on.Delete;
//                    dbs.Insert = on.Insert;
//                    dbs.LoggerFactory = LoggerFactory;
//                    this.DbChangeProcessors.Add(dbs);
//                }
//            }
//        }

//        public string[] GetTableNamesOfDbChangeProcessors()
//        {
//            return this.DbChangeProcessors.Select(x => x.Table).ToArray();
//        }

//        public List<IDbChangeProcessor> GetDbChangeProcessorsForInsert(string cacheName)
//        {
//            return this.DbChangeProcessors.Where(x => x.CacheName == cacheName && x.Insert).ToList();
//        }

//        public List<IDbChangeProcessor> GetDbChangeProcessorsForUpdate(string cacheName)
//        {
//            return this.DbChangeProcessors.Where(x => x.CacheName == cacheName && x.Update).ToList();
//        }

//        public List<IDbChangeProcessor> GetDbChangeProcessorsForDelete(string cacheName)
//        {
//            return this.DbChangeProcessors.Where(x => x.CacheName == cacheName && x.Delete).ToList();
//        }

//        private bool CheckTypeSerializable(Type type)
//        {
//            return type.GetCustomAttribute<SerializableAttribute>() != null;
//        }

//        public static CacheStoreConfiguration Instance(string configurationPath)
//        {
//            if (_instance == null)
//            {
//                lock(locker)
//                {
//                    if (_instance == null)
//                    {
//                        var ins = new CacheStoreConfiguration();
//                        ins.filePath = configurationPath;
//                        //_instance.fileWatcher = new System.IO.FileSystemWatcher(configurationPath);
//                        //_instance.fileWatcher.Changed += _instance.OnConfigChanged;
//                        ins.BuildUp();
//                        _instance = ins;
//                    }
//                }
//            }
//            return _instance;
//        }

//        internal bool HasDbNotifiers(string cacheName)
//        {
//            return this.DbChangeProcessors.Any(x => x.CacheName == cacheName);
//        }

//        public static CacheStoreConfiguration Instance()
//        {
//            var configDir = Frameworks.Common.Configurations.Configuration.Instance.Setting.RootPath + "\\Cfg";
//            return Instance(System.IO.Path.Combine(configDir, "CacheStore.config"));
//        }
//        public string CacheFactory { get; set; }
//        private Dictionary<string, CacheLoaderConfiguration> Loaders { get; set; }

//        public CacheLoaderConfiguration LoaderConfig(string cacheName)
//        {
//            CacheLoaderConfiguration config = null;
//            this.Loaders.TryGetValue(cacheName, out config);
//            return config;
//        }
//        public List<string> GetCacheNamesByTableNameForDbChangeNotifier(string tableName)
//        {
//            return this.DbChangeProcessors.Where(x => x.Table == tableName).Select(r => r.CacheName).ToList();
//        }
//        public List<IDbChangeProcessor> GetDbChangeStreamersForUpdate(string cacheName, string tableName)
//        {
//            return this.DbChangeProcessors.Where(x => x.Table == tableName && x.CacheName == cacheName && x.Update).ToList();
//        }

//        public List<IDbChangeProcessor> GetDbChangeStreamersForDelete(string cacheName, string tableName)
//        {
//            return this.DbChangeProcessors.Where(x => x.Table == tableName && x.CacheName == cacheName && x.Delete).ToList();
//        }

//        public List<IDbChangeProcessor> GetDbChangeStreamersForInsert(string cacheName, string tableName)
//        {
//            return this.DbChangeProcessors.Where(x => x.Table == tableName && x.CacheName == cacheName && x.Insert).ToList();
//        }

//        private class OnDesc
//        {
//            public string[] PrimaryKeys { get; internal set; }
//            public bool Delete { get; internal set; }
//            public bool Insert { get; internal set; }
//            public string Table { get; internal set; }
//            public bool Update { get; internal set; }
//        }
//    }
//}
