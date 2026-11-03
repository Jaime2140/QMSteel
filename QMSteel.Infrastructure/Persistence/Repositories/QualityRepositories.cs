using Microsoft.EntityFrameworkCore;
using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.Entities.Receiving;
using QMSteel.Domain.Entities.Suppliers;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.Standards;

namespace QMSteel.Infrastructure.Persistence.Repositories
{
    internal class SupplierRepository(QMSteelDbContext context) : Repository<Supplier>(context), ISupplierRepository
    {
        public Task<bool> ExistsByNameAsync(string name, CancellationToken ct = default) =>
            Set.AnyAsync(s => s.Name == name.Trim(), ct);

        public async Task<IReadOnlyList<Supplier>> GetAllAsync(SupplierStatus? status = null, SupplierCategory? category = null, CancellationToken ct = default) =>
            await Set.Where(s => (status == null || s.Status == status) && (category == null || s.Categories.Contains(category.Value)))
                .OrderBy(s => s.Name)
                .ToListAsync(ct);
    }

    internal class ReceivingInspectionRepository(QMSteelDbContext context) : Repository<ReceivingInspection>(context), IReceivingInspectionRepository
    {
        public Task<ReceivingInspection?> GetByFolioAsync(string folio, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(r => r.Folio == folio.Trim().ToUpper(), ct);

        // Trazabilidad de colada: en qué recibos entró un número de colada.
        public async Task<IReadOnlyList<ReceivingInspection>> GetByHeatNumberAsync(string heatNumber, CancellationToken ct = default)
        {
            var heat = heatNumber.Trim().ToUpper();
            return await Set.Where(r => r.Items.Any(i => i.HeatNumber == heat)).OrderByDescending(r => r.ReceivedOn).ToListAsync(ct);
        }

        public async Task<IReadOnlyList<ReceivingInspection>> GetBySupplierAsync(Guid supplierId, CancellationToken ct = default) =>
            await Set.Where(r => r.SupplierId == supplierId).OrderByDescending(r => r.ReceivedOn).ToListAsync(ct);
    }

    internal class NonConformityRepository(QMSteelDbContext context) : Repository<NonConformity>(context), INonConformityRepository
    {
        public Task<NonConformity?> GetByFolioAsync(string folio, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(n => n.Folio == folio.Trim().ToUpper(), ct);

        // La criticidad se guarda como texto; se ordena en memoria para respetar Alta > Media > Baja.
        public async Task<IReadOnlyList<NonConformity>> GetOpenAsync(CancellationToken ct = default)
        {
            var open = await Set.Where(n => n.Status != NonConformityStatus.Closed && n.Status != NonConformityStatus.Cancelled)
                .ToListAsync(ct);
            return open.OrderByDescending(n => n.Criticality).ThenBy(n => n.DetectedOn).ToList();
        }

        public async Task<IReadOnlyList<NonConformity>> GetDetectedBetweenAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
            await Set.Where(n => n.DetectedOn >= from && n.DetectedOn <= to).OrderBy(n => n.DetectedOn).ToListAsync(ct);
    }

    internal class AuditRepository(QMSteelDbContext context) : Repository<Audit>(context), IAuditRepository
    {
        public Task<Audit?> GetByFolioAsync(string folio, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(a => a.Folio == folio.Trim().ToUpper(), ct);

        public async Task<IReadOnlyList<Audit>> GetByYearAsync(int year, CancellationToken ct = default)
        {
            var (from, to) = YearRange(year);
            return await Set.Where(a => a.PlannedStart >= from && a.PlannedStart < to).OrderBy(a => a.PlannedStart).ToListAsync(ct);
        }

        internal static (DateOnly From, DateOnly To) YearRange(int year) => (new DateOnly(year, 1, 1), new DateOnly(year + 1, 1, 1));
    }

    internal class ManagementReviewRepository(QMSteelDbContext context) : Repository<ManagementReview>(context), IManagementReviewRepository
    {
        public async Task<IReadOnlyList<ManagementReview>> GetByYearAsync(int year, CancellationToken ct = default)
        {
            var (from, to) = AuditRepository.YearRange(year);
            return await Set.Where(r => r.HeldOn >= from && r.HeldOn < to).OrderBy(r => r.HeldOn).ToListAsync(ct);
        }
    }

    internal class InspectionRepository(QMSteelDbContext context) : Repository<Inspection>(context), IInspectionRepository
    {
        public Task<Inspection?> GetByFolioAsync(string folio, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(i => i.Folio == folio.Trim().ToUpper(), ct);

        // Historial de soldaduras recientes del expediente del soldador (RF-3).
        public async Task<IReadOnlyList<Inspection>> GetByWelderAsync(Guid welderId, int take = 20, CancellationToken ct = default) =>
            await Set.Where(i => i.Status == InspectionStatus.Signed && i.WeldItems.Any(w => w.WelderId == welderId))
                .OrderByDescending(i => i.InspectedOn)
                .Take(take)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Inspection>> GetSignedBetweenAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
            await Set.Where(i => i.Status == InspectionStatus.Signed && i.InspectedOn >= from && i.InspectedOn <= to)
                .OrderBy(i => i.InspectedOn)
                .ToListAsync(ct);

        public async Task<IReadOnlyList<Inspection>> GetByProjectAsync(string projectName, CancellationToken ct = default) =>
            await Set.Where(i => i.ProjectName == projectName.Trim()).OrderBy(i => i.InspectedOn).ToListAsync(ct);
    }

    internal class ComplianceRequirementRepository(QMSteelDbContext context) : Repository<ComplianceRequirement>(context), IComplianceRequirementRepository
    {
        public async Task<IReadOnlyList<ComplianceRequirement>> GetByStandardAsync(QualityStandard standard, bool onlyActive = true, CancellationToken ct = default) =>
            await Set.Where(r => r.Standard == standard && (!onlyActive || r.IsActive)).OrderBy(r => r.Clause).ToListAsync(ct);
    }

    internal class RequirementAssessmentRepository(QMSteelDbContext context) : Repository<RequirementAssessment>(context), IRequirementAssessmentRepository
    {
        public Task<RequirementAssessment?> GetByRequirementAsync(Guid requirementId, CancellationToken ct = default) =>
            Set.FirstOrDefaultAsync(a => a.RequirementId == requirementId, ct);

        public async Task<IReadOnlyList<RequirementAssessment>> GetAllAsync(CancellationToken ct = default) =>
            await Set.ToListAsync(ct);
    }

    internal class QualityObjectiveRepository(QMSteelDbContext context) : Repository<QualityObjective>(context), IQualityObjectiveRepository
    {
        public async Task<IReadOnlyList<QualityObjective>> GetByYearAsync(int year, CancellationToken ct = default) =>
            await Set.Where(o => o.Year == year).OrderBy(o => o.Name).ToListAsync(ct);
    }
}
