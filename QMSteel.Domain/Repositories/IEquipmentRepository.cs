using QMSteel.Domain.Entities.Equipments;

namespace QMSteel.Domain.Repositories;

public interface IEquipmentRepository : IRepository<Equipment>
{
    Task<Equipment?> GetByCodeAsync(string code, CancellationToken ct = default);
    Task<IReadOnlyList<Equipment>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default);
    Task<IReadOnlyList<Equipment>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default);
    Task<IReadOnlyList<Equipment>> GetCalibrationDueBeforeAsync(DateOnly date, CancellationToken ct = default);
}
