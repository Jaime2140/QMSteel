using QMSteel.Domain.Entities.Compliance;

namespace QMSteel.Domain.Repositories;

public interface IRequirementAssessmentRepository : IRepository<RequirementAssessment>
{
    Task<RequirementAssessment?> GetByRequirementAsync(Guid requirementId, CancellationToken ct = default);
    Task<IReadOnlyList<RequirementAssessment>> GetAllAsync(CancellationToken ct = default);
}
