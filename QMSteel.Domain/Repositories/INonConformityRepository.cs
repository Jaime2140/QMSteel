using QMSteel.Domain.Entities.NonConformities;

namespace QMSteel.Domain.Repositories;

public interface INonConformityRepository : IRepository<NonConformity>
{
    Task<NonConformity?> GetByFolioAsync(string folio, CancellationToken ct = default);
    Task<IReadOnlyList<NonConformity>> GetOpenAsync(CancellationToken ct = default);
    Task<IReadOnlyList<NonConformity>> GetDetectedBetweenAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
}
