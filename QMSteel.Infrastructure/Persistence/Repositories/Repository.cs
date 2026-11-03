using Microsoft.EntityFrameworkCore;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Repositories;

namespace QMSteel.Infrastructure.Persistence.Repositories
{
    // Los agregados se cargan completos: las colecciones hijas están configuradas con AutoInclude.
    internal class Repository<T> : IRepository<T> where T : Entity
    {
        protected readonly QMSteelDbContext Context;

        public Repository(QMSteelDbContext context)
        {
            Context = context;
        }

        protected DbSet<T> Set => Context.Set<T>();

        public Task<T?> GetByIdAsync(Guid id, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(e => e.Id == id, ct);

        public async Task AddAsync(T entity, CancellationToken ct = default) =>
            await Set.AddAsync(entity, ct);

        // Un agregado cargado por el repositorio ya está rastreado; Update solo aplica a uno desconectado.
        // Llamarlo sobre uno rastreado marcaría como modificadas las hijas nuevas en lugar de agregarlas.
        public void Update(T entity)
        {
            if (Context.Entry(entity).State == EntityState.Detached)
                Set.Update(entity);
        }

        public void Remove(T entity) => Set.Remove(entity);
    }
}
