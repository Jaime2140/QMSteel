using QMSteel.Domain.Entities.Audits;

namespace QMSteel.Domain.Repositories;

public interface IAuditRepository : IRepository<Audit>
{
    Task<Audit?> GetByFolioAsync(string folio, CancellationToken ct = default);
    Task<IReadOnlyList<Audit>> GetByYearAsync(int year, CancellationToken ct = default);
}
