using System;
using System.Threading.Tasks;
using CommonX.Logging;
using MassTransit;
using MassTransit.Pipeline;

namespace CommonX.ServiceBus.MassTransit.Observers
{
    public class PublishObserver :IPublishObserver
    {
        //private ILogger _logger;

        public PublishObserver(/*ILogger loggerFactory*/)
        {
           /* _logger = loggerFactory; *//*loggerFactory.Create(GetType());*/
        }

        public Task PrePublish<T>(PublishContext<T> context)
            where T : class
        {
            //if (_logger.IsAuditOn)
            //{
            //    _logger.Audit("PrePublish" + context.Message);
            //}
            // called right before the message is published (sent to exchange or topic)
            return Task.FromResult(0);
        }

        public Task PostPublish<T>(PublishContext<T> context)
            where T : class
        {
            //if (_logger.IsAuditOn)
            //{
            //    _logger.Audit("PostPublish" + context.Message);
            //}
            // called after the message is published (and acked by the broker if RabbitMQ)
            return Task.FromResult(0);
        }

        public Task PublishFault<T>(PublishContext<T> context, Exception exception)
            where T : class
        {
            //if (_logger.IsErrorOn)
            //{
            //    _logger.Error("PublishFault" + exception);
            //}
            // called if there was an exception publishing the message
            return Task.FromResult(0);
        }
    }
}
