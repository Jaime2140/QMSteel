using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;

namespace QMSteel.Domain.Entities.Compliance
{
    public enum ComplianceStatus { Compliant, PartiallyCompliant, NonCompliant, NotApplicable }

    // Estado de cumplimiento de un requisito en la planta; lo actualiza el sistema con base en los datos o un evaluador.
    public class RequirementAssessment : AggregateRoot
    {
        public Guid RequirementId { get; private set; }
        public ComplianceStatus Status { get; private set; }
        public bool IsAutomatic { get; private set; }
        public string? Notes { get; private set; }
        public DateOnly AssessedOn { get; private set; }
        public Guid? AssessedByUserId { get; private set; }

        private readonly List<Guid> _evidenceFileIds = new();
        public IReadOnlyCollection<Guid> EvidenceFileIds => _evidenceFileIds;

        private RequirementAssessment() { }

        public RequirementAssessment(Guid tenantId, Guid requirementId, ComplianceStatus status, DateOnly assessedOn,
            bool isAutomatic, Guid? assessedByUserId = null, string? notes = null)
        {
            if (requirementId == Guid.Empty)
                throw new DomainException("ASSESSMENT_REQUIREMENT_REQUIRED", "La evaluación requiere un requisito.");

            TenantId = tenantId;
            RequirementId = requirementId;
            Assess(status, assessedOn, isAutomatic, assessedByUserId, notes);
        }

        public void Assess(ComplianceStatus status, DateOnly assessedOn, bool isAutomatic, Guid? assessedByUserId = null, string? notes = null)
        {
            if (!isAutomatic && assessedByUserId is null)
                throw new DomainException("ASSESSMENT_USER_REQUIRED", "Una evaluación manual requiere al evaluador.");
            if (status is ComplianceStatus.NonCompliant or ComplianceStatus.NotApplicable && !isAutomatic && string.IsNullOrWhiteSpace(notes))
                throw new DomainException("ASSESSMENT_NOTES_REQUIRED", "Justifica el incumplimiento o la no aplicabilidad.");

            Status = status;
            AssessedOn = assessedOn;
            IsAutomatic = isAutomatic;
            AssessedByUserId = assessedByUserId;
            Notes = notes?.Trim();
        }

        public void AddEvidence(Guid fileId)
        {
            if (!_evidenceFileIds.Contains(fileId))
                _evidenceFileIds.Add(fileId);
        }
    }
}
