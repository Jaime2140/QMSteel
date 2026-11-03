using QMSteel.Domain.Entities.Documents;

namespace QMSteel.Domain.Repositories;

public interface IControlledDocumentRepository : IRepository<ControlledDocument>
{
    Task<ControlledDocument?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<ControlledDocument>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default);
    Task<IReadOnlyList<ControlledDocument>> GetReviewDueBeforeAsync(DateOnly date, CancellationToken ct = default);
}
