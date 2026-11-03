using QMSteel.Domain.Entities.Procedures;

namespace QMSteel.Domain.Repositories;

public interface IPqrRepository : IRepository<Pqr>
{
    Task<Pqr?> GetByNumberAsync(string number, CancellationToken ct = default);
    Task<IReadOnlyList<Pqr>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Pqr>> GetAllAsync(CancellationToken ct = default);
}
