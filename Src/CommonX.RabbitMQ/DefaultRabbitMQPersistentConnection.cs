
namespace CommonX.RabbitMQ
{
    using CommonX.Logging;
    using CommonX.Serializing;
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using global::RabbitMQ.Client.Exceptions;
    using Polly;
    using System;
    using System.IO;
    using System.Net.Sockets;
    using System.Text;

    public class DefaultRabbitMQPersistentConnection : IRabbitMQPersistentConnection
    {
        private readonly IConnectionFactory _connectionFactory;
        private readonly int _retryCount;
        private readonly ILogger _logger;
        private IJsonSerializer _jsonSerializer;
        IConnection _connection;
        bool _disposed;

        object sync_root = new object();

        public DefaultRabbitMQPersistentConnection(IConnectionFactory connectionFactory, ILoggerFactory loggerFactory, IJsonSerializer jsonSerializer, int retryCount = 5)
        {
            _jsonSerializer = jsonSerializer;
            _logger = loggerFactory.Create(GetType());
            _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
            _retryCount = retryCount;
        }

        public bool IsConnected
        {
            get
            {
                return _connection != null && _connection.IsOpen && !_disposed;
            }
        }

        public IModel CreateModel()
        {
            if (!IsConnected)
            {
                throw new InvalidOperationException("No RabbitMQ connections are available to perform this action");
            }

            return _connection.CreateModel();
        }

        public void Dispose()
        {
            if (_disposed) return;

            _disposed = true;

            try
            {
                _connection.Dispose();
            }
            catch (IOException ex)
            {
                _logger.Error(ex.ToString());
            }
        }
        public IConnection StartConnect()
        {
            lock (sync_root)
            {
                if (!IsConnected)
                {
                    _connection = _connectionFactory.CreateConnection();
                }

            }
            _logger.Info("RabbitMQ Client is  connecting");
            return _connection;
        }

        public bool TryConnect()
        {
            _logger.Info("RabbitMQ Client is trying to connect");

            lock (sync_root)
            {
                var policy = Policy.Handle<SocketException>()
                    .Or<BrokerUnreachableException>()
                    .WaitAndRetry(_retryCount, retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)), (ex, time) =>
                    {
                        _logger.Error(ex.ToString());
                    }
                );

                policy.Execute(() =>
                {
                    _connection = _connectionFactory
                          .CreateConnection();
                });

                if (IsConnected)
                {
                    _connection.ConnectionShutdown += OnConnectionShutdown;
                    _connection.CallbackException += OnCallbackException;
                    _connection.ConnectionBlocked += OnConnectionBlocked;

                    _logger.Info($"RabbitMQ persistent connection acquired a connection {_connection.Endpoint.HostName} and is subscribed to failure events");

                    return true;
                }
                else
                {
                    _logger.Warn("FATAL ERROR: RabbitMQ connections could not be created and opened");

                    return false;
                }
            }
        }

        private void OnConnectionBlocked(object sender, ConnectionBlockedEventArgs e)
        {
            if (_disposed) return;

            _logger.Warn("A RabbitMQ connection is shutdown. Trying to re-connect...");

            TryConnect();
        }

        void OnCallbackException(object sender, CallbackExceptionEventArgs e)
        {
            if (_disposed) return;

            _logger.Warn("A RabbitMQ connection throw exception. Trying to re-connect...");

            TryConnect();
        }

        void OnConnectionShutdown(object sender, ShutdownEventArgs reason)
        {
            if (_disposed) return;

            _logger.Warn("A RabbitMQ connection is on shutdown. Trying to re-connect...");

            TryConnect();
        }
        public bool Publish<T>(
            T info,
            string exchangeName,
            string queueName,
            string routkey,
            bool persistent = false,
            bool durableQueue = true,
            bool autoDelete = false,
            bool exclusive = false,
            ExchangeType exchangeType = ExchangeType.Default)
        {
            try
            {
                var conn = StartConnect();
                var channel = conn.CreateModel();
                if (exchangeType != ExchangeType.Default)
                {
                    string strExchangeType = ExchangeTypeDataDict.ExchangeTypeDict[exchangeType];
                    channel.ExchangeDeclare(exchangeName, strExchangeType);
                }



                channel.QueueDeclare(queue: queueName,
                   durable: durableQueue,
                   exclusive: exclusive,
                   autoDelete: autoDelete,
                   arguments: null);

                var properties = channel.CreateBasicProperties();
                properties.Persistent = true;

                var data = _jsonSerializer.Serialize(info);
                var body = Encoding.UTF8.GetBytes(data);

                channel.BasicPublish(exchange: exchangeName,
                     routingKey: routkey,
                     basicProperties: properties,
                      body: body);
                return true;
            }
            catch (Exception ex)
            {
                _logger.Error($"发布消息失败，异常是：{ex.Message},详细信息是：{ex.StackTrace}");
                return false;
            }

        }
        public void Subscribe(string queueName, Action<object, BasicDeliverEventArgs> HandleMessages, bool durable = true, bool exclusive = false, bool autoDelete = false, bool autoAck = true)
        {
            var conn = StartConnect();
            var channel = conn.CreateModel();
            channel.QueueDeclare(queue: queueName,
                                    durable: durable,
                                    exclusive: exclusive,
                                    autoDelete: autoDelete,
                                    arguments: null);


            var consumer = new EventingBasicConsumer(channel);
            consumer.Received += (model, ea) => 
            {
                HandleMessages?.Invoke(model, ea);
                channel.BasicAck(deliveryTag: ea.DeliveryTag, multiple: false);
            };

            channel.BasicConsume(queue: queueName,
                                 consumer: consumer);
        }
    }
}
