using QMSteel.Domain.Entities.Welders;

namespace QMSteel.Domain.Repositories;

public interface IWelderRepository : IRepository<Welder>
{
    Task<Welder?> GetByIdWithQualificationsAsync(Guid id, CancellationToken ct = default);
    Task<Welder?> GetByStampNumberAsync(string stampNumber, CancellationToken ct = default);
    Task<bool> ExistsByEmployeeNumberAsync(string employeeNumber, CancellationToken ct = default);
    Task<IReadOnlyList<Welder>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default);
}