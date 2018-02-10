using CommonX.EventBus;
using CommonX.EventBus.Abstractions;
using CommonX.EventBus.Events;
using CommonX.Logging;
using Confluent.Kafka;
using System;
using System.Collections.Generic;
using System.Text;
using CommonX.JsonNet;
using CommonX.Components;
using CommonX.Serializing;
using Polly.Retry;
using Polly;
using System.Threading.Tasks;

namespace CommonX.Kafka.Impl
{
    [Component]
    public class EventBusKafka : IEventBus, IDisposable
    {
        public void Dispose()
        {
            throw new NotImplementedException();
        }

        public void Publish(IntegrationEvent @event)
        {
            throw new NotImplementedException();
        }

        public void Subscribe<T, TH>(Func<TH> handler)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            throw new NotImplementedException();
        }

        public void Unsubscribe<T, TH>()
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>
        {
            throw new NotImplementedException();
        }
    }
}

