using QMSteel.Domain.Entities.Compliance;

namespace QMSteel.Domain.Repositories;

public interface IQualityObjectiveRepository : IRepository<QualityObjective>
{
    Task<IReadOnlyList<QualityObjective>> GetByYearAsync(int year, CancellationToken ct = default);
}
