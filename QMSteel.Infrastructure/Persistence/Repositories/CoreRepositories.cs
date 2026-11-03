using Microsoft.EntityFrameworkCore;
using QMSteel.Domain.Entities.Documents;
using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Entities.Tenants;
using QMSteel.Domain.Entities.Users;
using QMSteel.Domain.Repositories;

namespace QMSteel.Infrastructure.Persistence.Repositories
{
    internal class TenantRepository(QMSteelDbContext context) : Repository<Tenant>(context), ITenantRepository
    {
        public Task<Tenant?> GetBySlugAsync(string slug, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(t => t.Slug == slug.Trim().ToLower(), ct);
    }

    internal class UserRepository(QMSteelDbContext context) : Repository<User>(context), IUserRepository
    {
        // Al iniciar sesión todavía no se conoce el tenant: la identidad se busca en todos.
        public Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken ct = default) =>
            Set.IgnoreQueryFilters().FirstOrDefaultAsync(u => u.IdentityId == identityId, ct);

        public Task<User?> GetByEmailAsync(string email, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(u => u.Email == email.Trim().ToLower(), ct);

        public async Task<IReadOnlyList<User>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            await Set.Where(u => onlyActive == null || u.IsActive == onlyActive).OrderBy(u => u.FullName).ToListAsync(ct);
    }

    internal class ControlledDocumentRepository(QMSteelDbContext context) : Repository<ControlledDocument>(context), IControlledDocumentRepository
    {
        public Task<ControlledDocument?> GetByCodeAsync(string code, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(d => d.Code == code.Trim().ToUpper(), ct);

        public async Task<IReadOnlyList<ControlledDocument>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            await Set.Where(d => onlyActive == null || d.IsActive == onlyActive).OrderBy(d => d.Code).ToListAsync(ct);

        public async Task<IReadOnlyList<ControlledDocument>> GetReviewDueBeforeAsync(DateOnly date, CancellationToken ct = default) =>
            await Set.Where(d => d.IsActive && d.Revisions.Any(r => r.Status == RevisionStatus.Effective && r.NextReviewOn < date))
                .OrderBy(d => d.Code)
                .ToListAsync(ct);
    }

    internal class EquipmentRepository(QMSteelDbContext context) : Repository<Equipment>(context), IEquipmentRepository
    {
        public Task<Equipment?> GetByCodeAsync(string code, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(e => e.Code == code.Trim().ToUpper(), ct);

        public async Task<IReadOnlyList<Equipment>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idList = ids.Distinct().ToList();
            return await Set.Where(e => idList.Contains(e.Id)).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Equipment>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            await Set.Where(e => onlyActive == null || e.IsActive == onlyActive).OrderBy(e => e.Code).ToListAsync(ct);

        // Incluye los equipos activos que nunca se han calibrado.
        public async Task<IReadOnlyList<Equipment>> GetCalibrationDueBeforeAsync(DateOnly date, CancellationToken ct = default) =>
            await Set.Where(e => e.IsActive
                    && (!e.Calibrations.Any()
                        || e.Calibrations.OrderByDescending(c => c.CalibratedOn).Select(c => c.DueOn).First() < date))
                .OrderBy(e => e.Code)
                .ToListAsync(ct);
    }
}
