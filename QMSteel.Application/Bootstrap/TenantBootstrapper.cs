using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Tenants;
using QMSteel.Domain.Entities.Users;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Bootstrap
{
    // Planta y administrador iniciales para poder entrar al sistema la primera vez.
    public sealed class BootstrapOptions
    {
        public const string SectionName = "Bootstrap";

        public bool Enabled { get; set; }
        public string TenantName { get; set; } = string.Empty;
        public string TenantSlug { get; set; } = string.Empty;
        public string AdminIdentityId { get; set; } = string.Empty;
        public string AdminEmail { get; set; } = string.Empty;
        public string AdminFullName { get; set; } = string.Empty;
    }

    // Idempotente: si la planta o el administrador ya existen, no los vuelve a crear.
    public sealed class TenantBootstrapper
    {
        private const int TrialDays = 30;

        private readonly ITenantRepository _tenants;
        private readonly IUserRepository _users;
        private readonly IUnitOfWork _unitOfWork;
        private readonly TimeProvider _time;

        public TenantBootstrapper(ITenantRepository tenants, IUserRepository users, IUnitOfWork unitOfWork, TimeProvider time)
        {
            _tenants = tenants;
            _users = users;
            _unitOfWork = unitOfWork;
            _time = time;
        }

        public async Task RunAsync(BootstrapOptions options, CancellationToken ct = default)
        {
            if (!options.Enabled)
                return;

            var tenant = await _tenants.GetBySlugAsync(options.TenantSlug, ct);
            if (tenant is null)
            {
                tenant = new Tenant(options.TenantName, options.TenantSlug, BillingCycle.Monthly, _time.Today().AddDays(TrialDays));
                await _tenants.AddAsync(tenant, ct);
            }

            if (await _users.GetByIdentityIdAsync(options.AdminIdentityId, ct) is null)
                await _users.AddAsync(new User(tenant.Id, options.AdminIdentityId, options.AdminEmail, options.AdminFullName,
                    UserRole.Administrator), ct);

            await _unitOfWork.SaveChangesAsync(ct);
        }
    }
}
