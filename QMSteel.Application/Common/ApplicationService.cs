using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Application.Common
{
    // Base de los servicios de aplicación. Las reglas de negocio las aplica el dominio (DomainException);
    // aquí solo se resuelve lo que no le toca: existencia, duplicados, firma y guardado.
    public abstract class ApplicationService
    {
        protected ApplicationService(UseCaseContext context)
        {
            Context = context;
        }

        protected UseCaseContext Context { get; }

        protected DateOnly Today => Context.Time.Today();

        protected DateTime UtcNow => Context.Time.UtcNow();

        protected Guid TenantId => Context.CurrentUser.TenantId
            ?? throw new InvalidOperationException("La petición no tiene tenant.");

        protected Guid UserId => Context.CurrentUser.UserId
            ?? throw new InvalidOperationException("La petición no tiene usuario.");

        protected Task SaveAsync(CancellationToken ct) => Context.UnitOfWork.SaveChangesAsync(ct);

        protected static Result<TOut> NotFound<T, TOut>(Guid id) => Result.Failure<TOut>(ApplicationErrors.NotFound<T>(id));

        protected static async Task<Result<TOut>> GetAsync<T, TOut>(IRepository<T> repository, Guid id, Func<T, TOut> map, CancellationToken ct)
            where T : AggregateRoot
        {
            var entity = await repository.GetByIdAsync(id, ct);
            return entity is null ? NotFound<T, TOut>(id) : map(entity);
        }

        // Carga el agregado, aplica el cambio, guarda y regresa el resultado mapeado.
        protected async Task<Result<TOut>> ChangeAsync<T, TOut>(IRepository<T> repository, Guid id, Action<T> change, Func<T, TOut> map,
            CancellationToken ct) where T : AggregateRoot
        {
            var entity = await repository.GetByIdAsync(id, ct);
            if (entity is null)
                return NotFound<T, TOut>(id);

            change(entity);
            repository.Update(entity);
            await SaveAsync(ct);
            return map(entity);
        }

        // Igual que ChangeAsync, pero antes obtiene la firma electrónica del usuario en sesión.
        protected async Task<Result<TOut>> SignAndChangeAsync<T, TOut>(IRepository<T> repository, Guid id, string meaning, bool requireApprover,
            Action<T, ElectronicSignature> change, Func<T, TOut> map, CancellationToken ct) where T : AggregateRoot
        {
            var signature = await Context.Signatures.SignAsync(meaning, requireApprover, ct);
            if (signature.IsFailure)
                return Result.Failure<TOut>(signature.Error);

            return await ChangeAsync(repository, id, entity => change(entity, signature.Value), map, ct);
        }

        protected async Task<Result<TOut>> AddAsync<T, TOut>(IRepository<T> repository, T entity, Func<T, TOut> map, CancellationToken ct)
            where T : AggregateRoot
        {
            await repository.AddAsync(entity, ct);
            await SaveAsync(ct);
            return map(entity);
        }
    }
}
