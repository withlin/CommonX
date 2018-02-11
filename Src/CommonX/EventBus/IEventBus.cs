using CommonX.EventBus.Abstractions;
using CommonX.EventBus.Events;
using System;

namespace CommonX.EventBus
{
    public interface IEventBus
    {
        void Subscribe<T, TH>(Func<TH> handler)
            where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;
        void Unsubscribe<T, TH>()
             where T : IntegrationEvent
            where TH : IIntegrationEventHandler<T>;

        void Publish(IntegrationEvent @event);
    }
}
