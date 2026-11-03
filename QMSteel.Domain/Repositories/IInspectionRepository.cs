using QMSteel.Domain.Entities.Inspections;

namespace QMSteel.Domain.Repositories;

public interface IInspectionRepository : IRepository<Inspection>
{
    Task<Inspection?> GetByFolioAsync(string folio, CancellationToken ct = default);
    Task<IReadOnlyList<Inspection>> GetByWelderAsync(Guid welderId, int take = 20, CancellationToken ct = default);
    Task<IReadOnlyList<Inspection>> GetSignedBetweenAsync(DateOnly from, DateOnly to, CancellationToken ct = default);
    Task<IReadOnlyList<Inspection>> GetByProjectAsync(string projectName, CancellationToken ct = default);
}
