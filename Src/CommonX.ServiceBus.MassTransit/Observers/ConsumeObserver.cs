using System;
using System.Threading.Tasks;
using CommonX.Components;
using CommonX.Logging;
using MassTransit;
using MassTransit.Pipeline;

namespace CommonX.ServiceBus.MassTransit.Observers
{
    /// <summary>
    /// A consume observer
    /// </summary>
    [Component]
    public class ConsumeObserver :IConsumeObserver
    {
        private ILogger _logger;

        public ConsumeObserver(ILoggerFactory loggerFactory)
        {
            _logger = loggerFactory.Create(GetType());
        }

        Task IConsumeObserver.PreConsume<T>(ConsumeContext<T> context)
        {
            // called before the consumer's Consume method is called
            if (_logger.IsDebugEnabled)
            {
                _logger.Info("PreConsume" + context.Message);
            }
            return Task.FromResult(0);
        }

        Task IConsumeObserver.PostConsume<T>(ConsumeContext<T> context)
        {
            // called after the consumer's Consume method is called
            // if an exception was thrown, the ConsumeFault method is called instead
            if (_logger.IsDebugEnabled)
            {
                _logger.Info("PostConsume" + context.Message);
            }
            return Task.FromResult(0);
        }

        Task IConsumeObserver.ConsumeFault<T>(ConsumeContext<T> context, Exception exception)
        {
            // called if the consumer's Consume method throws an exception
            _logger.Error(exception);
            return Task.FromResult(0);
        }
    }
}
