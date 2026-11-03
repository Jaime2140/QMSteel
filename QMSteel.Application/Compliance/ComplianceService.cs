using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.Standards;

namespace QMSteel.Application.Compliance
{
    // RF-10: matriz de cumplimiento normativo. El catálogo de requisitos es global; la evaluación es por planta.
    public sealed class ComplianceService : ApplicationService
    {
        private readonly IComplianceRequirementRepository _requirements;
        private readonly IRequirementAssessmentRepository _assessments;

        public ComplianceService(UseCaseContext context, IComplianceRequirementRepository requirements,
            IRequirementAssessmentRepository assessments) : base(context)
        {
            _requirements = requirements;
            _assessments = assessments;
        }

        // clausePrefix permite el detalle al dar clic en un punto de la norma (ej. "11" para AISC 207 Cap. 11).
        public async Task<ComplianceMatrixDto> GetMatrixAsync(QualityStandard standard, string? clausePrefix = null, CancellationToken ct = default)
        {
            var requirements = (await _requirements.GetByStandardAsync(standard, true, ct))
                .Where(r => string.IsNullOrWhiteSpace(clausePrefix) || r.Clause.StartsWith(clausePrefix.Trim(), StringComparison.OrdinalIgnoreCase))
                .ToList();
            var assessments = (await _assessments.GetAllAsync(ct)).ToDictionary(a => a.RequirementId);

            var rows = requirements.Select(r =>
            {
                var assessment = assessments.GetValueOrDefault(r.Id);
                return new ComplianceRowDto(r.Id, r.Edition, r.Clause, r.Title, r.EvidenceSource, assessment?.Id, assessment?.Status,
                    assessment?.AssessedOn, assessment?.IsAutomatic ?? false, assessment?.Notes, assessment?.EvidenceFileIds.ToList() ?? []);
            }).ToList();

            var evaluated = rows.Where(r => r.Status is not null).Select(r => r.Status!.Value).ToList();
            return new ComplianceMatrixDto(standard, clausePrefix, QualityKpis.CompliancePercentage(evaluated), rows.Count,
                rows.Count - evaluated.Count, rows);
        }

        public async Task<Result<ComplianceRowDto>> AssessAsync(Guid requirementId, AssessRequirementRequest request, CancellationToken ct = default)
        {
            var requirement = await _requirements.GetByIdAsync(requirementId, ct);
            if (requirement is null || !requirement.IsActive)
                return Result.Failure<ComplianceRowDto>(ApplicationErrors.NotFound<ComplianceRequirement>(requirementId));

            var assessedOn = request.AssessedOn ?? Today;
            var assessment = await _assessments.GetByRequirementAsync(requirementId, ct);
            if (assessment is null)
            {
                assessment = new RequirementAssessment(TenantId, requirementId, request.Status, assessedOn, false, UserId, request.Notes);
                await _assessments.AddAsync(assessment, ct);
            }
            else
            {
                assessment.Assess(request.Status, assessedOn, false, UserId, request.Notes);
                _assessments.Update(assessment);
            }

            await SaveAsync(ct);
            return Row(requirement, assessment);
        }

        public async Task<Result<ComplianceRowDto>> AddEvidenceAsync(Guid requirementId, Guid fileId, CancellationToken ct = default)
        {
            var requirement = await _requirements.GetByIdAsync(requirementId, ct);
            var assessment = await _assessments.GetByRequirementAsync(requirementId, ct);
            if (requirement is null || assessment is null)
                return Result.Failure<ComplianceRowDto>(Error.Validation("COMPLIANCE_NOT_ASSESSED", "Evalúa el requisito antes de adjuntar evidencia."));

            assessment.AddEvidence(fileId);
            _assessments.Update(assessment);
            await SaveAsync(ct);
            return Row(requirement, assessment);
        }

        private static ComplianceRowDto Row(ComplianceRequirement r, RequirementAssessment a) => new(r.Id, r.Edition, r.Clause, r.Title,
            r.EvidenceSource, a.Id, a.Status, a.AssessedOn, a.IsAutomatic, a.Notes, a.EvidenceFileIds.ToList());
    }
}
