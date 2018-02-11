using CommonX.EventBus.Events;
using System.Threading.Tasks;

namespace CommonX.EventBus.Abstractions
{
    public interface IIntegrationEventHandler<in TIntegrationEvent> : IIntegrationEventHandler
        where TIntegrationEvent : IntegrationEvent
    {
        Task Handle(TIntegrationEvent @event);
    }
    public interface IIntegrationEventHandler
    {

    }
}
