using QMSteel.Domain.Entities.Personnel;

namespace QMSteel.Domain.Repositories;

public interface IInspectorRepository : IRepository<Inspector>
{
    Task<Inspector?> GetByUserIdAsync(Guid userId, CancellationToken ct = default);
    Task<bool> ExistsByEmployeeNumberAsync(string employeeNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Inspector>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default);
}
