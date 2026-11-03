using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Tenants;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.Standards;

namespace QMSteel.Application.Users
{
    // Datos de la planta en sesión y normas habilitadas. La suscripción la administra la operación del SaaS, no la planta.
    public sealed class TenantService : ApplicationService
    {
        private readonly ITenantRepository _tenants;

        public TenantService(UseCaseContext context, ITenantRepository tenants) : base(context)
        {
            _tenants = tenants;
        }

        public async Task<Result<TenantDto>> GetCurrentAsync(CancellationToken ct = default)
        {
            var tenant = await _tenants.GetByIdAsync(TenantId, ct);
            return tenant is null ? NotFound<Tenant, TenantDto>(TenantId) : TenantDto.From(tenant, Today);
        }

        public Task<Result<TenantDto>> EnableStandardAsync(QualityStandard standard, CancellationToken ct = default) =>
            ChangeCurrentAsync(t => t.EnableStandard(standard), ct);

        public Task<Result<TenantDto>> DisableStandardAsync(QualityStandard standard, CancellationToken ct = default) =>
            ChangeCurrentAsync(t => t.DisableStandard(standard), ct);

        private async Task<Result<TenantDto>> ChangeCurrentAsync(Action<Tenant> change, CancellationToken ct)
        {
            var tenant = await _tenants.GetByIdAsync(TenantId, ct);
            if (tenant is null)
                return NotFound<Tenant, TenantDto>(TenantId);

            change(tenant);
            _tenants.Update(tenant);
            await SaveAsync(ct);
            return TenantDto.From(tenant, Today);
        }
    }
}
