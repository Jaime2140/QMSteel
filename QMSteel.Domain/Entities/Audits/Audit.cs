using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Standards;

namespace QMSteel.Domain.Entities.Audits
{
    public enum AuditType { Internal, External }

    public enum AuditStatus { Planned, InProgress, Completed, Cancelled }

    public enum ParticipantRole { LeadAuditor, Auditor, Auditee }

    public enum FindingClassification { Observation, OpportunityForImprovement, MinorNonConformity, MajorNonConformity }

    public sealed record AuditScheduled(Guid AuditId, DateOnly PlannedStart, IReadOnlyList<string> ParticipantEmails, DateTime OccurredAtUtc) : IDomainEvent;

    public sealed record AuditNonConformityRaised(Guid AuditId, Guid FindingId, FindingClassification Classification,
        string Clause, string Description, DateTime OccurredAtUtc) : IDomainEvent;

    public class AuditParticipant : Entity
    {
        public string Name { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public ParticipantRole Role { get; private set; }
        public Guid? UserId { get; private set; }

        private AuditParticipant() { }

        internal AuditParticipant(string name, string email, ParticipantRole role, Guid? userId)
        {
            Name = name;
            Email = email;
            Role = role;
            UserId = userId;
        }
    }

    public class AuditFinding : Entity
    {
        public int Number { get; private set; }
        public string Clause { get; private set; } = default!;
        public string Description { get; private set; } = default!;
        public FindingClassification Classification { get; private set; }
        public Guid? NonConformityId { get; private set; }

        public bool IsNonConformity => Classification is FindingClassification.MinorNonConformity or FindingClassification.MajorNonConformity;

        private AuditFinding() { }

        internal AuditFinding(int number, string clause, string description, FindingClassification classification)
        {
            Number = number;
            Clause = clause;
            Description = description;
            Classification = classification;
        }

        internal void LinkNonConformity(Guid nonConformityId) => NonConformityId = nonConformityId;
    }

    public class Audit : AggregateRoot
    {
        public string Folio { get; private set; } = default!;
        public AuditType Type { get; private set; }
        public QualityStandard Standard { get; private set; }
        public string Scope { get; private set; } = default!;
        public DateOnly PlannedStart { get; private set; }
        public DateOnly PlannedEnd { get; private set; }
        public AuditStatus Status { get; private set; } = AuditStatus.Planned;
        public Guid? ReportFileId { get; private set; }
        public int? RequirementsEvaluated { get; private set; }
        public int? RequirementsCompliant { get; private set; }

        private readonly List<AuditParticipant> _participants = new();
        public IReadOnlyCollection<AuditParticipant> Participants => _participants;

        private readonly List<AuditFinding> _findings = new();
        public IReadOnlyCollection<AuditFinding> Findings => _findings;

        private Audit() { }

        public Audit(Guid tenantId, string folio, AuditType type, QualityStandard standard, string scope, DateOnly plannedStart, DateOnly plannedEnd)
        {
            if (string.IsNullOrWhiteSpace(folio))
                throw new DomainException("AUDIT_FOLIO_REQUIRED", "El folio de la auditoría es obligatorio.");
            if (string.IsNullOrWhiteSpace(scope))
                throw new DomainException("AUDIT_SCOPE_REQUIRED", "El alcance de la auditoría es obligatorio.");

            TenantId = tenantId;
            Folio = folio.Trim().ToUpperInvariant();
            Type = type;
            Standard = standard;
            Scope = scope.Trim();
            SetDates(plannedStart, plannedEnd);
        }

        public void AddParticipant(string name, string email, ParticipantRole role, Guid? userId = null)
        {
            EnsureNotFinished();
            if (string.IsNullOrWhiteSpace(name) || string.IsNullOrWhiteSpace(email))
                throw new DomainException("AUDIT_PARTICIPANT_REQUIRED", "El participante requiere nombre y correo.");
            if (role == ParticipantRole.LeadAuditor && _participants.Any(p => p.Role == ParticipantRole.LeadAuditor))
                throw new DomainException("AUDIT_LEAD_DUPLICATED", "La auditoría ya tiene auditor líder.");

            _participants.Add(new AuditParticipant(name.Trim(), email.Trim().ToLowerInvariant(), role, userId));
        }

        public void RemoveParticipant(Guid participantId)
        {
            EnsureNotFinished();
            _participants.RemoveAll(p => p.Id == participantId);
        }

        // Dispara el aviso por correo a auditores y auditados.
        public void Schedule(DateTime occurredAtUtc)
        {
            if (Status != AuditStatus.Planned)
                throw new DomainException("AUDIT_NOT_PLANNED", "Solo se notifica una auditoría planeada.");
            if (!_participants.Any(p => p.Role == ParticipantRole.LeadAuditor))
                throw new DomainException("AUDIT_LEAD_REQUIRED", "Asigna un auditor líder.");

            Raise(new AuditScheduled(Id, PlannedStart, _participants.Select(p => p.Email).ToList(), occurredAtUtc));
        }

        public void Reschedule(DateOnly plannedStart, DateOnly plannedEnd, DateTime occurredAtUtc)
        {
            if (Status != AuditStatus.Planned)
                throw new DomainException("AUDIT_NOT_PLANNED", "Solo se reprograma una auditoría planeada.");
            SetDates(plannedStart, plannedEnd);
            Raise(new AuditScheduled(Id, PlannedStart, _participants.Select(p => p.Email).ToList(), occurredAtUtc));
        }

        public void Start()
        {
            if (Status != AuditStatus.Planned)
                throw new DomainException("AUDIT_NOT_PLANNED", "La auditoría ya inició o terminó.");
            Status = AuditStatus.InProgress;
        }

        // Un hallazgo de NC mayor/menor dispara automáticamente la solicitud en el módulo de no conformidades.
        public AuditFinding AddFinding(string clause, string description, FindingClassification classification, DateTime occurredAtUtc)
        {
            if (Status != AuditStatus.InProgress)
                throw new DomainException("AUDIT_NOT_IN_PROGRESS", "Solo se registran hallazgos durante la auditoría.");
            if (string.IsNullOrWhiteSpace(clause))
                throw new DomainException("AUDIT_FINDING_CLAUSE_REQUIRED", "Indica la cláusula de la norma.");
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("AUDIT_FINDING_DESCRIPTION_REQUIRED", "Describe el hallazgo.");

            var finding = new AuditFinding(_findings.Count + 1, clause.Trim(), description.Trim(), classification);
            _findings.Add(finding);

            if (finding.IsNonConformity)
                Raise(new AuditNonConformityRaised(Id, finding.Id, classification, finding.Clause, finding.Description, occurredAtUtc));

            return finding;
        }

        public void LinkFindingToNonConformity(Guid findingId, Guid nonConformityId)
        {
            var finding = _findings.SingleOrDefault(f => f.Id == findingId)
                ?? throw new DomainException("AUDIT_FINDING_NOT_FOUND", "El hallazgo no existe.");
            if (!finding.IsNonConformity)
                throw new DomainException("AUDIT_FINDING_NOT_NC", "Solo los hallazgos de no conformidad se ligan a una NC.");
            finding.LinkNonConformity(nonConformityId);
        }

        public void Complete(Guid reportFileId, int requirementsEvaluated, int requirementsCompliant)
        {
            if (Status != AuditStatus.InProgress)
                throw new DomainException("AUDIT_NOT_IN_PROGRESS", "La auditoría no está en curso.");
            if (requirementsEvaluated <= 0 || requirementsCompliant < 0 || requirementsCompliant > requirementsEvaluated)
                throw new DomainException("AUDIT_REQUIREMENTS_INVALID", "Los requisitos evaluados y cumplidos no son consistentes.");

            ReportFileId = reportFileId;
            RequirementsEvaluated = requirementsEvaluated;
            RequirementsCompliant = requirementsCompliant;
            Status = AuditStatus.Completed;
        }

        public void Cancel()
        {
            EnsureNotFinished();
            Status = AuditStatus.Cancelled;
        }

        public decimal? CompliancePercentage => RequirementsEvaluated is > 0
            ? decimal.Round(100m * RequirementsCompliant!.Value / RequirementsEvaluated.Value, 1)
            : null;

        private void SetDates(DateOnly plannedStart, DateOnly plannedEnd)
        {
            if (plannedEnd < plannedStart)
                throw new DomainException("AUDIT_INVALID_DATES", "La fecha de fin no puede ser anterior al inicio.");
            PlannedStart = plannedStart;
            PlannedEnd = plannedEnd;
        }

        private void EnsureNotFinished()
        {
            if (Status is AuditStatus.Completed or AuditStatus.Cancelled)
                throw new DomainException("AUDIT_FINISHED", "La auditoría ya terminó o fue cancelada.");
        }
    }
}
