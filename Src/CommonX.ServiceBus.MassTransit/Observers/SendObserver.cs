using System;
using System.Threading.Tasks;
using CommonX.Logging;
using MassTransit;

namespace CommonX.ServiceBus.MassTransit.Observers
{
    public class SendObserver :
    ISendObserver
    {
        //private ILogger _logger;

        public SendObserver(/*ILogger loggerFactory*/)
        {
            //_logger = loggerFactory;
        }
        public Task PreSend<T>(SendContext<T> context)
            where T : class
        {
            // called just before a message is sent, all the headers should be setup and everything
            //if (_logger.IsAuditOn)
            //{
            //    _logger.Audit("PreSend" + context.Message);
            //}
            return Task.FromResult(0);
        }

        public Task PostSend<T>(SendContext<T> context)
            where T : class
        {
            // called just after a message it sent to the transport and acknowledged (RabbitMQ)
            //if (_logger.IsAuditOn)
            //{
            //    _logger.Audit("PostSend" + context.Message);
            //}
            ////开票单需要监控发送
            //_logger.Info("PostSend" + context.Message);
            return Task.FromResult(0);
        }

        public Task SendFault<T>(SendContext<T> context, Exception exception)
            where T : class
        {
            // called if an exception occurred sending the message
            //if (_logger.IsErrorOn)
            //{
            //    _logger.Error("SendFault" + exception);
            //}
            return Task.FromResult(0);
        }
    }
}