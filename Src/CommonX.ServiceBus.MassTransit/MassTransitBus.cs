using CommonX.Logging;
using MassTransit;
using MassTransit.RabbitMqTransport.Contexts;
using MassTransit.RabbitMqTransport.Topology;
using RabbitMQ.Client;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading;

namespace CommonX.ServiceBus.MassTransit
{
    /// <summary>
    ///     Represents the message bus.
    /// </summary>
    public class MassTransitBus : IBus
    {
        private readonly global::MassTransit.IBus _bus;

        public MassTransitBus(global::MassTransit.IBus bus)
        {
            _bus = bus;
        }

        public void Publish<T>(T message) where T : class
        {
            _bus.Publish(message);
        }

        public void Publish(object message)
        {
            _bus.Publish(message);
        }
        public void OrderPublish<T>(T message, CancellationToken cancelSend) where T : class
        {
            OrderSend(message, cancelSend);
        }

        /// <summary>
        ///     Sends the message.
        /// </summary>
        /// <param name="destination">
        ///     The address of the destination to which the message will be sent.
        /// </param>
        /// <param name="message">The message to send.</param>
        public void Send(string destination, object message, bool isFullUri = false)
        {
            var se = _bus.GetSendEndpoint(Utils.GetFullUri(destination, isFullUri)).Result;
            se.Send(message);
        }

        public void OrderSend<T>(string destination, T message, CancellationToken cancelSend, bool isFullUri = false) where T : class
        {
            OrderSend(message, cancelSend,Utils.GetFullUri(destination, isFullUri));
        }
        public void OrderSend<T>(string serviceEndPoint, string branchId, T message, CancellationToken cancelSend) where T : class
        {
            string destination = $"{serviceEndPoint}-{branchId}";
            OrderSend<T>(destination, message, cancelSend,true );
        }

        //public void Send(string destination, string correlationId, object message)
        //{
        //    var se = _bus.GetSendEndpoint(Utils.GetFullUri(destination)).Result;
        //    se.Send(message);
        //}

        public void Send(string serviceEndPoint, string branchId, object message)
        {
            string destination = $"{serviceEndPoint}-{branchId}";
            Send(destination, message, true);
        }


        #region private 

        private void OrderSend<T>(T message, CancellationToken cancelSend, Uri destination = null)
            where T : class
        {
            if (message == null)
                throw new ArgumentNullException(nameof(message));
            var factory = new ConnectionFactory
            {
                HostName = Configurations.Configuration.Instance.Setting.TransportIp.Split(',')[0],
                UserName = Configurations.Configuration.Instance.Setting.TransportUserName,
                Password = Configurations.Configuration.Instance.Setting.TransportPassword,
            };
            if (!string.IsNullOrEmpty(Configurations.Configuration.Instance.Setting.TransportVirtualHost))
            {
                factory.VirtualHost = Configurations.Configuration.Instance.Setting.TransportVirtualHost;
            }
            factory.AutomaticRecoveryEnabled = true;
            using (var connection = factory.CreateConnection(Configurations.Configuration.Instance.Setting.TransportIp.Split(',')))
            {
                using (var channel = connection.CreateModel())
                {
                    var properties = channel.CreateBasicProperties();
                    var setting =
                        new RabbitMqSendSettings(
                            destination == null ? (typeof(T).Namespace + ":" + typeof(T).Name) : destination.AbsolutePath.Replace("/", ""),
                            "", true, false);
                    var exchange = Configurations.Configuration.Instance.Setting.TransportExchange;
                    var context = new BasicPublishRabbitMqSendContext<T>(properties, exchange, message, cancelSend);
                    properties.ContentType = "application/vnd.masstransit+json";

                    try
                    {
                        var headers = context.Headers.GetAll()
                            .Where(x => (x.Value != null) && (x.Value is string || x.Value.GetType().IsValueType))
                            .ToArray();

                        if (properties.Headers == null)
                            properties.Headers = new Dictionary<string, object>(headers.Length);

                        foreach (var header in headers)
                        {
                            if (properties.Headers.ContainsKey(header.Key))
                                continue;

                            properties.SetHeader(header.Key, header.Value);
                        }
                        properties.Headers["Content-Type"] = "application/vnd.masstransit+json";
                        properties.Persistent = context.Durable;

                        if (context.MessageId.HasValue)
                            properties.MessageId = context.MessageId.ToString();

                        if (context.CorrelationId.HasValue)
                            properties.CorrelationId = context.CorrelationId.ToString();

                        if (context.TimeToLive.HasValue)
                            properties.Expiration = context.TimeToLive.Value.TotalMilliseconds.ToString("F0",
                                CultureInfo.InvariantCulture);
                        channel.BasicPublish(context.Exchange, context.RoutingKey, context.Mandatory,
                            context.BasicProperties, context.Body);
                    }
                    catch (Exception ex)
                    {
                    }
                }
            }
        }
        #endregion
    }
}
