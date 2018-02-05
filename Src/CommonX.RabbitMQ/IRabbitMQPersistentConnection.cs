namespace Shared.MiddleWare.RabbitMQ
{
    using global::RabbitMQ.Client;
    using global::RabbitMQ.Client.Events;
    using System;

    public interface IRabbitMQPersistentConnection
        : IDisposable
    {

        bool IsConnected { get; }
        bool TryConnect();

        IModel CreateModel();
        IConnection StartConnect();
        bool Publish<T>(
            T info,
            string exchangeName,
            string queueName,
            string routkey,
            bool persistent = false,
            bool durableQueue = true,
            bool autoDelete = false,
            bool exclusive = false,
            ExchangeType exchangeType = ExchangeType.Default);

        void Subscribe(string queueName, Action<object, BasicDeliverEventArgs> HandleMessages,bool durable = true, bool exclusive = false, bool autoDelete = false, bool autoAck = true);
    }
}
