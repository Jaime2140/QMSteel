using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Standards;

namespace QMSteel.Domain.Repositories;

public interface IComplianceRequirementRepository : IRepository<ComplianceRequirement>
{
    Task<IReadOnlyList<ComplianceRequirement>> GetByStandardAsync(QualityStandard standard, bool onlyActive = true, CancellationToken ct = default);
}
