using QMSteel.Application.Common;
using QMSteel.Application.Compliance;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Application.Dashboard
{
    // RF-10: tablero ejecutivo con los KPIs de calidad del periodo.
    public sealed class DashboardService : ApplicationService
    {
        private readonly ITenantRepository _tenants;
        private readonly IAuditRepository _audits;
        private readonly IInspectionRepository _inspections;
        private readonly INonConformityRepository _nonConformities;
        private readonly IEquipmentRepository _equipments;
        private readonly IQualityObjectiveRepository _objectives;
        private readonly ComplianceService _compliance;

        public DashboardService(UseCaseContext context, ITenantRepository tenants, IAuditRepository audits, IInspectionRepository inspections,
            INonConformityRepository nonConformities, IEquipmentRepository equipments, IQualityObjectiveRepository objectives,
            ComplianceService compliance) : base(context)
        {
            _tenants = tenants;
            _audits = audits;
            _inspections = inspections;
            _nonConformities = nonConformities;
            _equipments = equipments;
            _objectives = objectives;
            _compliance = compliance;
        }

        public async Task<ExecutiveDashboardDto> GetExecutiveAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        {
            var inspections = await _inspections.GetSignedBetweenAsync(from, to, ct);
            var nonConformities = await _nonConformities.GetDetectedBetweenAsync(from, to, ct);
            var openNonConformities = await _nonConformities.GetOpenAsync(ct);
            var equipments = await _equipments.GetAllAsync(true, ct);
            var objectives = await _objectives.GetByYearAsync(to.Year, ct);

            var equipmentStatuses = equipments.Select(e => e.GetStatus(Today)).ToList();

            return new ExecutiveDashboardDto(from, to, await ComplianceByStandardAsync(from, to, ct),
                QualityKpis.DirectAcceptancePercentage(inspections), QualityKpis.RejectionPercentage(inspections),
                openNonConformities.Count, QualityKpis.AverageDaysToClose(nonConformities),
                QualityKpis.ClosureEffectivenessPercentage(nonConformities),
                MoneyDto.From(QualityKpis.CostOfPoorQuality(nonConformities, Currency.USD))!,
                MoneyDto.From(QualityKpis.CostOfPoorQuality(nonConformities, Currency.MXN))!,
                QualityKpis.CalibratedEquipmentPercentage(equipments, Today),
                equipmentStatuses.Count(s => s is ResourceStatus.Valid or ResourceStatus.ExpiringSoon),
                equipmentStatuses.Count(s => s is ResourceStatus.Expired or ResourceStatus.Blocked),
                objectives.Select(QualityObjectiveDto.From).ToList());
        }

        // Una fila por norma habilitada para la planta: resultado de auditorías del periodo y de la matriz de cumplimiento.
        private async Task<IReadOnlyList<StandardComplianceDto>> ComplianceByStandardAsync(DateOnly from, DateOnly to, CancellationToken ct)
        {
            var tenant = await _tenants.GetByIdAsync(TenantId, ct);
            var audits = new List<Audit>();
            for (var year = from.Year; year <= to.Year; year++)
                audits.AddRange(await _audits.GetByYearAsync(year, ct));

            var completed = audits.Where(a => a.Status == AuditStatus.Completed && a.PlannedStart >= from && a.PlannedStart <= to).ToList();

            var rows = new List<StandardComplianceDto>();
            foreach (var standard in tenant?.EnabledStandards ?? [])
            {
                var ofStandard = completed.Where(a => a.Standard == standard).ToList();
                decimal? auditPercentage = ofStandard.Count == 0
                    ? null
                    : QualityKpis.Percentage(ofStandard.Sum(a => a.RequirementsCompliant ?? 0), ofStandard.Sum(a => a.RequirementsEvaluated ?? 0));

                var matrix = await _compliance.GetMatrixAsync(standard, null, ct);
                rows.Add(new StandardComplianceDto(standard, auditPercentage, matrix.CompliancePercentage));
            }

            return rows;
        }
    }
}
