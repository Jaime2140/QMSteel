using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Welding;

namespace QMSteel.Domain.Repositories;

public interface IWpsRepository : IRepository<Wps>
{
    Task<Wps?> GetByNumberAsync(string number, CancellationToken ct = default);
    Task<IReadOnlyList<Wps>> GetAllAsync(WeldingProcess? process = null, WpsStatus? status = null, CancellationToken ct = default);
}
