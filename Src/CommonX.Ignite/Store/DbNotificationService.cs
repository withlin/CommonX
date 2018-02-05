//using System;
//using System.Collections.Generic;
//using System.Collections.Specialized;
//using System.Configuration;
//using System.Linq;
//using System.Text;
//using Apache.Ignite.Core;
//using Apache.Ignite.Core.Datastream;
//using System.Threading.Tasks;
//using System.Threading;
//using ECommon.Logging;
//using RabbitMQ.Client;
//using RabbitMQ.Client.Events;
//using ECommon.Components;
//using Newtonsoft.Json;

//namespace ECommon.Cache.Ignite.Store
//{
//    public class DbNotificationService : IDbNotificationService
//    {
//        public static Dictionary<string, long> dictInfo = new Dictionary<string, long>();

//        public static Dictionary<string, List<Type>> tableCaches =
//            new Dictionary<string, List<Type>>();

//        private static readonly string ExchangeName =
//            ((NameValueCollection)ConfigurationManager.GetSection("IgniteConfig"))["DbNotificationExchange"] + "";

//        private static readonly Dictionary<string, IDataStreamer<string, OggEntry>> dictDs =
//            new Dictionary<string, IDataStreamer<string, OggEntry>>();


//        private static int maxHandlingTask = 0;
//        static DbNotificationService()
//        {
//            maxHandlingTask = (int)Math.Floor(Environment.ProcessorCount * 1.0 / 2.0);
//        }

//        private readonly ICacheConfiguration _cacheConfiguration;
//        private readonly Dictionary<string, string> queueConsumerTags = new Dictionary<string, string>();
//        private ILogger _logger;
//        private string[] _tableNames;

//        private IModel channel;
//        private IConnection connection;
//        private EventingBasicConsumer consumer;
//        private ConnectionFactory factory;
//        private IIgnite ignite;
//        private string moduleName = string.Empty;

//        public DbNotificationService(ICacheConfiguration cacheConfiguration)
//        {
//            _cacheConfiguration = cacheConfiguration;
//            if (ConfigurationManager.AppSettings["dbnotification-should-purge"] != null)
//            {
//                bool sp = false;
//                bool.TryParse(ConfigurationManager.AppSettings["dbnotification-should-purge"], out sp);
//                this.shouldPurgeQueue = sp;
//            }
//        }

//        public EventHandler<BasicDeliverEventArgs> MessageReceived { get; set; }

//        public void Start(string moduleName, string[] tableNames, string hostName, string userName,
//            string password)
//        {
//            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//            {
//                ignite = scope.Resolve<IIgnite>();
//                _logger = scope.Resolve<ILoggerFactory>().Create("DbNotificationService");
//                _tableNames = tableNames;
//                this.moduleName = moduleName;
//                factory = new ConnectionFactory();
//                factory.HostName = hostName;
//                factory.UserName = userName;
//                factory.Password = password;
//                factory.AutomaticRecoveryEnabled = true;
//                factory.RequestedConnectionTimeout = 15000; //15秒
//                startChannel();
//            }
//        }
//        private bool shouldPurgeQueue = false;
//        public void ReConnect()
//        {
//            queueConsumerTags.Clear();
//            if (connection != null)
//            {
//                lock (connection)
//                {
//                    if (connection != null)
//                    {
//                        connection.ConnectionShutdown -= Connection_ConnectionShutdown;
//                        connection.ConnectionBlocked -= Connection_ConnectionBlocked;
//                        connection.ConnectionUnblocked -= Connection_ConnectionUnblocked;

//                        try { connection.Close(5000); } catch { }
//                        try { connection.Abort(5000); } catch { }
//                        try { connection.Dispose(); } catch { }
//                        connection = null;
//                    }
//                }
//            }
//            shouldPurgeQueue = true;
//            try
//            {
//                startChannel();
//            }
//            catch (Exception ex)
//            {
//                WriteConsoleLog("Reconnect failed for:" + ex.Message + ". Please rerun reconnect");
//            }
//            shouldPurgeQueue = false;
//        }

//        private void WriteConsoleLog(string info, int topOffset = 0)
//        {
//            //int top = System.Console.CursorTop;
//            //var color = System.Console.ForegroundColor;
//            //var curTop = System.Console.WindowTop + 1 + topOffset;
//            //System.Console.SetCursorPosition(0, curTop);
//            //System.Console.Write(new string(' ', 120));
//            //System.Console.SetCursorPosition(0, curTop);
//            //System.Console.ForegroundColor = ConsoleColor.Red;
//            //System.Console.Write(info);
//            //System.Console.SetCursorPosition(0, top);
//            //System.Console.ForegroundColor = color;
//            Console.WriteLine(info);
//        }

//        private string GetConsumerName(string tableName)
//        {
//            return moduleName + "-" + tableName;
//        }

//        private string getConsumerTag(string tableName)
//        {
//            var consumerName = GetConsumerName(tableName);
//            string tag = null;
//            queueConsumerTags.TryGetValue(consumerName, out tag);
//            return tag;
//        }

//        private void AddConsumer(string tableName)
//        {
//            var consumerName = GetConsumerName(tableName);
//            if (getConsumerTag(tableName) == null)
//            {
//                var m_ConsumerTag = channel.BasicConsume(consumerName, false, consumer);
//                queueConsumerTags.Add(consumerName, m_ConsumerTag);
//            }
//        }
//        private void startChannel()
//        {
//            WriteConsoleLog("Connecting to DbNofication MessageQueue:" + factory.HostName);
//            connection = factory.CreateConnection("ERP CacheIgnite Server Console");
//            connection.ConnectionBlocked += Connection_ConnectionBlocked;
//            connection.ConnectionUnblocked += Connection_ConnectionUnblocked;
//            connection.ConnectionShutdown += Connection_ConnectionShutdown;
//            channel = connection.CreateModel();
//            var qos = Math.Max((ushort)Math.Ceiling(maxHandlingTask * 1.0 / _tableNames.Count()), (ushort)5);

//            channel.BasicQos(0, qos, false);
//            //申明交换器（如果交换器已经存在，能自动忽略）
//            channel.ExchangeDeclare(ExchangeName, ExchangeType.Direct, true, false, null);
//            //申明队列（如果队列已经存在，自动忽略）
//            foreach (var table in _tableNames)
//            {
//                var qn = moduleName + "-" + table;
//                channel.QueueDeclare(qn, true, false, false, null);
//                channel.QueueBind(qn, ExchangeName, table);
//                try
//                {
//                    if (shouldPurgeQueue)
//                        channel.QueuePurge(qn);
//                }
//                catch (Exception ex)
//                {

//                }
//            }
//            //绑定通用缓存
//            CommonBinding();
//            consumer = new EventingBasicConsumer(channel);
//            //绑定事件
//            consumer.Received += InternalMessageReceivedHandler;
//            //开始接受消费
//            foreach (var table in _tableNames)
//                AddConsumer(table);
//            foreach (var table in tableCaches.Keys)
//                AddConsumer(table);
//            WriteConsoleLog("Connected to DbNofication MessageQueue:" + factory.HostName);
//        }

//        private void Connection_ConnectionShutdown(object sender, ShutdownEventArgs e)
//        {
//            Console.WriteLine("RabbitMQ connection was shutdown for reason:" + e.ReplyText + ". Now Reconnect.");
//            ReConnect();
//        }

//        private void Connection_ConnectionUnblocked(object sender, EventArgs e)
//        {
//            Console.WriteLine("RabbitMQ connection UNBLOCKED");
//            ReConnect();
//        }

//        private void Connection_ConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
//        {
//            Console.WriteLine("RabbitMQ connection blocked for reason:" + e.Reason);
//        }

//        public void StopConsume()
//        {
//            //if (channel != null && channel.IsOpen && !string.IsNullOrEmpty(m_ConsumerTag))
//            //{
//            //    channel.BasicCancel(m_ConsumerTag);
//            //}
//        }

//        public void StartConsume()
//        {
//            //if (channel != null)
//            //{
//            //startChannel();
//            //m_ConsumerTag = channel.BasicConsume(queueName, false, consumer);
//            //}
//        }
//        Semaphore taskCountSema = new Semaphore(maxHandlingTask, maxHandlingTask * 2);
//        private void InternalMessageReceivedHandler(object model, BasicDeliverEventArgs r)
//        {
//            if (connection == null || connection != null && !connection.IsOpen) return;
//            var msg = Encoding.UTF8.GetString(r.Body);
//            var dd = new DbNotificationMessageHandleTaskArgs
//            {
//                service = this,
//                message = msg,
//                ackArgs = r,
//                parallelControl = this.taskCountSema
//            };
//            taskCountSema.WaitOne(-1);
//            Task.Factory.StartNew((s) =>
//            {
//                var _that = s as DbNotificationMessageHandleTaskArgs;
//                _that.PK = "";
//                _that.errorMessage = string.Empty;
//                var tmp = JsonConvert.DeserializeObject<OggEntryOriginal>(_that.message);
//                var entry = OggParser.Parse(tmp);
//                if (entry == null) { _that.errorMessage = "no changed!"; return; }
//                entry.JsonMessage = _that.message;
//                if (entry.Before.ContainsKey("PK"))
//                    _that.PK = entry.Before["PK"].ToString();
//                if (entry.After.ContainsKey("PK") && string.IsNullOrEmpty(_that.PK))
//                    _that.PK = entry.After["PK"].ToString();
//                var info = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "|" + "message received from:" +
//                           (r.RoutingKey == null ? "-" : r.RoutingKey);
//                WriteConsoleLog(info);
//                var retry = 3;
//                while (retry > 0)
//                {
//                    try
//                    {
//                        OggEntryHandling(entry, r.DeliveryTag);
//                        retry = -1;
//                    }
//                    catch (Exception ex)
//                    {
//                        _that.errorMessage = ex.Message;
//                        retry -= 1;
//                    }
//                    _that.retryTimes = retry;
//                }
//            }, dd).ContinueWith((t) =>
//            {
//                var state = t.AsyncState as DbNotificationMessageHandleTaskArgs;
//                var availableExecSeats = state.parallelControl.Release(1) + 1;
//                state.service.channel.BasicAck(r.DeliveryTag, false);
//                WriteConsoleLog(
//                    DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss") + "|" + "message acked from:" + (r.RoutingKey == null ? string.Empty : r.RoutingKey) + ", pk:" +
//                    (state.PK == null ? string.Empty : state.PK) + ". process "
//                    + (state.retryTimes == -1 ? "OK." : "FAILED for " + state.errorMessage)
//                    + (":" + availableExecSeats.ToString())
//                    , 1);
//            });

//            //Task.Factory.StartNew((s) =>
//            //{
//            //    var that = (s as DbNotificationMessageHandleTaskArgs).service;
//            //    that.MessageReceived(model, (s as DbNotificationMessageHandleTaskArgs).ackArgs);
//            //}, dd);
//        }

//        private void OggEntryHandling(OggEntry entry, ulong deliveryTag)
//        {
//            var csc = CacheStoreConfiguration.Instance();
//            //获取cacheName关联的TableName
//            var cacheNames = csc.GetCacheNamesByTableNameForDbChangeNotifier(entry.Table);
//            var existedIgniteCacheNames = ignite.GetCacheNames();
//            if (cacheNames.Any())
//                foreach (var s in cacheNames)
//                {
//                    //如果是不存在的cache，直接跳过。避免出错
//                    if (!existedIgniteCacheNames.Contains(s)) continue;

//                    switch (entry.Op)
//                    {
//                        case DbOperation.INSERT:
//                            var insertTasks = (from handler in CacheStoreConfiguration.Instance().GetDbChangeProcessorsForInsert(s)
//                                               where handler.Table == entry.Table && entry.Op == DbOperation.INSERT
//                                               select Task.Factory.StartNew(new Action<object>(handler.Execute), entry)).ToArray();
//                            if (insertTasks.Length > 0) Task.WaitAll(insertTasks);
//                            break;
//                        case DbOperation.UPDATE:
//                            var updateTasks = (from handler in CacheStoreConfiguration.Instance().GetDbChangeProcessorsForUpdate(s)
//                                               where handler.Table == entry.Table && entry.Op == DbOperation.UPDATE
//                                               select Task.Factory.StartNew(new Action<object>(handler.Execute), entry)).ToArray();
//                            if (updateTasks.Length > 0) Task.WaitAll(updateTasks);
//                            break;
//                        case DbOperation.DELETE:
//                            var deleteTasks = (from handler in CacheStoreConfiguration.Instance().GetDbChangeProcessorsForDelete(s)
//                                               where handler.Table == entry.Table && entry.Op == DbOperation.DELETE
//                                               select Task.Factory.StartNew(new Action<object>(handler.Execute), entry)).ToArray();
//                            if (deleteTasks.Length > 0) Task.WaitAll(deleteTasks);
//                            break;
//                        default:
//                            break;
//                    }

//                    //try
//                    //{
//                    //    if (!dictDs.ContainsKey(s))
//                    //        lock (dictDs)
//                    //        {
//                    //            if (!dictDs.ContainsKey(s)) dictDs.Add(s, ignite.GetDataStreamer<string, OggEntry>(s));
//                    //        }
//                    //    var ds = dictDs[s];
//                    //    if (ds.Receiver == null ||
//                    //        !ds.Receiver.GetType().IsInstanceOfType(typeof(OggNotificationReceiver)))
//                    //        ds.Receiver = new OggNotificationReceiver(s);
//                    //    ds.AddData(string.Join("-", entry.Table, entry.Op, entry.TS.ToString("")), entry);
//                    //    ds.Flush();
//                    //}
//                    //catch (Exception ex)
//                    //{
//                    //    Console.WriteLine(ex);
//                    //    _logger.Error(ex);
//                    //}
//                }
//            //更新通用缓存
//            if (tableCaches.ContainsKey(entry.Table))
//            {
//                var processors = tableCaches[entry.Table];
//                foreach (var p in processors)
//                {
//                    using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//                    {
//                        var proc = scope.Resolve(p) as ICommonDbChangeProcessor;
//                        proc?.Execute(entry);
//                    }
//                }
//            }

//            //
//            long count = 0;
//            if (dictInfo.ContainsKey(entry.Table))
//            {
//                count = dictInfo[entry.Table];
//                count = count + 1;
//                dictInfo[entry.Table] = count;
//            }
//            //
//        }

//        /// <summary>
//        ///     可能有一个表有多个处理程序
//        /// </summary>
//        public void CommonBinding()
//        {
//            using (var scope = ObjectContainer.Current.BeginLifetimeScope())
//            {
//                //var config = CacheConfiguration.Instance.CacheConfigurations;
//                var config = _cacheConfiguration.Instance.CacheConfigurations;
//                foreach (var cacheConfig in config)
//                    if (!string.IsNullOrEmpty(cacheConfig.Name))
//                    {
//                        var entityType = Type.GetType(cacheConfig.Name.Trim());
//                        if (entityType == null ||
//                            !(entityType.BaseType == typeof(BaseObject) ||
//                              entityType.BaseType == typeof(BaseEntityObject) ||
//                              entityType.BaseType.BaseType == typeof(BaseObject) ||
//                              entityType.BaseType.BaseType == typeof(BaseEntityObject))) continue;
//                        var loaderInterface = typeof(IDbChangeProcessor<,>).MakeGenericType(typeof(long), entityType);
//                        var loader = scope.Resolve(loaderInterface) as ICommonDbChangeProcessor;

//                        var result = loader?.Binding(channel, moduleName, ExchangeName).ToList().Distinct();
//                        foreach (var s in result ?? new List<string>())
//                            if (!tableCaches.ContainsKey(s))
//                                tableCaches.Add(s, new List<Type> { loaderInterface });
//                            else
//                                tableCaches[s].Add(loaderInterface);
//                    }
//            }
//        }

//        public void Dispose()
//        {
//            WriteConsoleLog("DbNotification service disposed!");
//        }
//    }
//    internal class DbNotificationMessageHandleTaskArgs
//    {
//        public DbNotificationService service;
//        public string message;
//        public BasicDeliverEventArgs ackArgs;
//        public int retryTimes;
//        public string errorMessage;
//        public string PK;
//        public Semaphore parallelControl;
//    }
//}