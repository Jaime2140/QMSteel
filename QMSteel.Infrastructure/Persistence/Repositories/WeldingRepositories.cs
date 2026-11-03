using Microsoft.EntityFrameworkCore;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Entities.Training;
using QMSteel.Domain.Entities.Welders;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.Welding;

namespace QMSteel.Infrastructure.Persistence.Repositories
{
    internal class WelderRepository(QMSteelDbContext context) : Repository<Welder>(context), IWelderRepository
    {
        public Task<Welder?> GetByIdWithQualificationsAsync(Guid id, CancellationToken ct = default) => GetByIdAsync(id, ct);

        public Task<Welder?> GetByStampNumberAsync(string stampNumber, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(w => w.StampNumber == stampNumber.Trim().ToUpper(), ct);

        public Task<bool> ExistsByEmployeeNumberAsync(string employeeNumber, CancellationToken ct = default) =>
            Set.AnyAsync(w => w.EmployeeNumber == employeeNumber.Trim(), ct);

        public async Task<IReadOnlyList<Welder>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            await Set.Where(w => onlyActive == null || w.IsActive == onlyActive).OrderBy(w => w.FullName).ToListAsync(ct);
    }

    internal class WpsRepository(QMSteelDbContext context) : Repository<Wps>(context), IWpsRepository
    {
        public Task<Wps?> GetByNumberAsync(string number, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(w => w.Number == number.Trim().ToUpper(), ct);

        public async Task<IReadOnlyList<Wps>> GetAllAsync(WeldingProcess? process = null, WpsStatus? status = null, CancellationToken ct = default) =>
            await Set.Where(w => (process == null || w.Variables.Process == process) && (status == null || w.Status == status))
                .OrderBy(w => w.Number)
                .ToListAsync(ct);
    }

    internal class PqrRepository(QMSteelDbContext context) : Repository<Pqr>(context), IPqrRepository
    {
        public Task<Pqr?> GetByNumberAsync(string number, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(p => p.Number == number.Trim().ToUpper(), ct);

        public async Task<IReadOnlyList<Pqr>> GetByIdsAsync(IEnumerable<Guid> ids, CancellationToken ct = default)
        {
            var idList = ids.Distinct().ToList();
            return await Set.Where(p => idList.Contains(p.Id)).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<Pqr>> GetAllAsync(CancellationToken ct = default) =>
            await Set.OrderBy(p => p.Number).ToListAsync(ct);
    }

    internal class InspectorRepository(QMSteelDbContext context) : Repository<Inspector>(context), IInspectorRepository
    {
        public Task<Inspector?> GetByUserIdAsync(Guid userId, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(i => i.UserId == userId, ct);

        public Task<bool> ExistsByEmployeeNumberAsync(string employeeNumber, CancellationToken ct = default) =>
            Set.AnyAsync(i => i.EmployeeNumber == employeeNumber.Trim(), ct);

        public async Task<IReadOnlyList<Inspector>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            await Set.Where(i => onlyActive == null || i.IsActive == onlyActive).OrderBy(i => i.FullName).ToListAsync(ct);
    }

    internal class TrainingPlanRepository(QMSteelDbContext context) : Repository<TrainingPlan>(context), ITrainingPlanRepository
    {
        public Task<TrainingPlan?> GetByYearAsync(int year, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(p => p.Year == year, ct);
    }

    internal class TrainingSessionRepository(QMSteelDbContext context) : Repository<TrainingSession>(context), ITrainingSessionRepository
    {
        public async Task<IReadOnlyList<TrainingSession>> GetBetweenAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
            await Set.Where(s => s.HeldOn >= from && s.HeldOn <= to).OrderBy(s => s.HeldOn).ToListAsync(ct);

        public async Task<IReadOnlyList<TrainingSession>> GetByEmployeeNumberAsync(string employeeNumber, CancellationToken ct = default)
        {
            var number = employeeNumber.Trim();
            return await Set.Where(s => s.Attendees.Any(a => a.EmployeeNumber == number))
                .OrderByDescending(s => s.HeldOn)
                .ToListAsync(ct);
        }
    }
}
