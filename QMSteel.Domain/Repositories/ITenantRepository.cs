using QMSteel.Domain.Entities.Tenants;

namespace QMSteel.Domain.Repositories;

public interface ITenantRepository : IRepository<Tenant>
{
    Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default);
}
