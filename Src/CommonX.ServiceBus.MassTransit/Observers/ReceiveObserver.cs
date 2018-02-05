using System;
using System.IO;
using System.Threading.Tasks;
using CommonX.Logging;
using MassTransit;

namespace CommonX.ServiceBus.MassTransit.Observers
{
    /// <summary>
    /// observe received messages immediately after they are delivered by the transport,
    /// </summary>
    public class ReceiveObserver : IReceiveObserver
    {
        private ILogger _logger;

        public ReceiveObserver(ILogger loggerFactory)
        {
            _logger = loggerFactory/*.Create(GetType());*/;
        }

        public Task PreReceive(ReceiveContext context)
        {
            // called immediately after the message was delivery by the transportif (_logger.IsAuditOn)
            if (_logger.IsDebugEnabled)
            {
                using (var body = context.GetBody())
                {
                    StreamReader reader = new StreamReader(body);
                    string text = reader.ReadToEnd();
                    _logger.Debug("PreReceive" + text);
                }
            }
            return Task.FromResult(0);
        }

        public Task PostReceive(ReceiveContext context)
        {
            // called after the message has been received and processed
            if (_logger.IsDebugEnabled)
            {
                using (var body = context.GetBody())
                {
                    StreamReader reader = new StreamReader(body);
                    string text = reader.ReadToEnd();
                    _logger.Info("PostReceive" + text);
                }
            }
            return Task.FromResult(0);
        }

        public Task PostConsume<T>(ConsumeContext<T> context, TimeSpan duration, string consumerType)
            where T : class
        {
            // called when the message was consumed, once for each consumer
            if (_logger.IsDebugEnabled)
            {
                _logger.Info("PostConsume" + context.Message);
            }
            return Task.FromResult(0);
        }

        public Task ConsumeFault<T>(ConsumeContext<T> context, TimeSpan elapsed, string consumerType,
            Exception exception) where T : class
        {
            // called when the message is consumed but the consumer throws an exception
            if (_logger.IsDebugEnabled)
            {
                _logger.Error(exception);
            }
            return Task.FromResult(0);
        }

        public Task ReceiveFault(ReceiveContext context, Exception exception)
        {
            // called when an exception occurs early in the message processing, such as deserialization, etc.
            if (_logger.IsDebugEnabled)
            {
                _logger.Error(exception);
            }
            return Task.FromResult(0);
        }
    }
}