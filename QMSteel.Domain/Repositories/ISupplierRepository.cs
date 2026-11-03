using QMSteel.Domain.Entities.Suppliers;

namespace QMSteel.Domain.Repositories;

public interface ISupplierRepository : IRepository<Supplier>
{
    Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default);
    Task<IReadOnlyList<Supplier>> GetAllAsync(SupplierStatus? status = null, SupplierCategory? category = null, CancellationToken ct = default);
}
