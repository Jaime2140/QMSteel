using QMSteel.Domain.Entities.Receiving;

namespace QMSteel.Domain.Repositories;

public interface IReceivingInspectionRepository : IRepository<ReceivingInspection>
{
    Task<ReceivingInspection?> GetByFolioAsync(string folio, CancellationToken ct = default);
    Task<IReadOnlyList<ReceivingInspection>> GetByHeatNumberAsync(string heatNumber, CancellationToken ct = default);
    Task<IReadOnlyList<ReceivingInspection>> GetBySupplierAsync(Guid supplierId, CancellationToken ct = default);
}
