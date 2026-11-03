using QMSteel.Domain.Entities.Audits;

namespace QMSteel.Domain.Repositories;

public interface IManagementReviewRepository : IRepository<ManagementReview>
{
    Task<IReadOnlyList<ManagementReview>> GetByYearAsync(int year, CancellationToken ct = default);
}
