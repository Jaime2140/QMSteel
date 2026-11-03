using Microsoft.Extensions.DependencyInjection;
using QMSteel.Application.Abstractions;
using QMSteel.Domain.Entities.Common;

namespace QMSteel.Infrastructure.Persistence
{
    // Resuelve del contenedor todos los IDomainEventHandler<T> registrados para el tipo concreto del evento.
    internal class DomainEventDispatcher : IDomainEventDispatcher
    {
        private readonly IServiceProvider _serviceProvider;

        public DomainEventDispatcher(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task DispatchAsync(IDomainEvent domainEvent, CancellationToken ct = default)
        {
            var handlerType = typeof(IDomainEventHandler<>).MakeGenericType(domainEvent.GetType());
            var handleMethod = handlerType.GetMethod(nameof(IDomainEventHandler<IDomainEvent>.HandleAsync))!;

            foreach (var handler in _serviceProvider.GetServices(handlerType))
                await (Task)handleMethod.Invoke(handler, new object[] { domainEvent, ct })!;
        }
    }
}
