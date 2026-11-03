using QMSteel.Domain.Entities.Common;

namespace QMSteel.Application.Abstractions
{
    public interface IDomainEventHandler<in TEvent> where TEvent : IDomainEvent
    {
        Task HandleAsync(TEvent domainEvent, CancellationToken ct = default);
    }

    public interface IDomainEventDispatcher
    {
        Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default);
    }
}
