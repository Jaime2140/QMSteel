using QMSteel.Application.Abstractions;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Repositories;

namespace QMSteel.Infrastructure.Persistence
{
    // Despacha los eventos de dominio antes de guardar, así sus efectos (continuidad, NC automáticas...)
    // quedan en la misma transacción que el cambio que los originó.
    internal class UnitOfWork : IUnitOfWork
    {
        private const int MaxDispatchRounds = 10;

        private readonly QMSteelDbContext _context;
        private readonly IDomainEventDispatcher _dispatcher;

        public UnitOfWork(QMSteelDbContext context, IDomainEventDispatcher dispatcher)
        {
            _context = context;
            _dispatcher = dispatcher;
        }

        public async Task<int> SaveChangesAsync(CancellationToken ct = default)
        {
            // Un manejador puede generar eventos nuevos en otros agregados: se repite hasta que no quede ninguno.
            for (var round = 0; ; round++)
            {
                var aggregates = _context.ChangeTracker.Entries<AggregateRoot>()
                    .Select(e => e.Entity)
                    .Where(a => a.DomainEvents.Count > 0)
                    .ToList();

                if (aggregates.Count == 0)
                    break;
                if (round == MaxDispatchRounds)
                    throw new InvalidOperationException("Los eventos de dominio siguen generando eventos nuevos; revisa si hay un ciclo entre manejadores.");

                var events = aggregates.SelectMany(a => a.DomainEvents).ToList();
                aggregates.ForEach(a => a.ClearDomainEvents());

                foreach (var domainEvent in events)
                    await _dispatcher.DispatchAsync(domainEvent, ct);
            }

            return await _context.SaveChangesAsync(ct);
        }
    }
}
