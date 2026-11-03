using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Audits
{
    public enum ManagementReviewStatus { Draft, Approved }

    public class ReviewDecision : Entity
    {
        public string Description { get; private set; } = default!;
        public string ResponsibleName { get; private set; } = default!;
        public DateOnly? DueOn { get; private set; }
        public DateOnly? CompletedOn { get; private set; }

        private ReviewDecision() { }

        internal ReviewDecision(string description, string responsibleName, DateOnly? dueOn)
        {
            Description = description;
            ResponsibleName = responsibleName;
            DueOn = dueOn;
        }

        internal void Complete(DateOnly completedOn) => CompletedOn = completedOn;
    }

    // Acta de Revisión por la Dirección: minuta, análisis de riesgos y decisiones estratégicas.
    public class ManagementReview : AggregateRoot
    {
        public string Title { get; private set; } = default!;
        public DateOnly HeldOn { get; private set; }
        public Guid? MinutesFileId { get; private set; }
        public Guid? RiskAnalysisFileId { get; private set; }
        public ManagementReviewStatus Status { get; private set; } = ManagementReviewStatus.Draft;
        public ElectronicSignature? ApprovedBy { get; private set; }

        private readonly List<string> _attendees = new();
        public IReadOnlyCollection<string> Attendees => _attendees;

        private readonly List<ReviewDecision> _decisions = new();
        public IReadOnlyCollection<ReviewDecision> Decisions => _decisions;

        private ManagementReview() { }

        public ManagementReview(Guid tenantId, string title, DateOnly heldOn, IEnumerable<string> attendees)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("REVIEW_TITLE_REQUIRED", "El título del acta es obligatorio.");

            TenantId = tenantId;
            Title = title.Trim();
            HeldOn = heldOn;
            SetAttendees(attendees);
        }

        public void SetAttendees(IEnumerable<string> attendees)
        {
            EnsureDraft();
            var list = attendees.Where(a => !string.IsNullOrWhiteSpace(a)).Select(a => a.Trim()).Distinct().ToList();
            if (list.Count == 0)
                throw new DomainException("REVIEW_ATTENDEES_REQUIRED", "Registra a los asistentes.");
            _attendees.Clear();
            _attendees.AddRange(list);
        }

        public void AttachMinutes(Guid fileId)
        {
            EnsureDraft();
            MinutesFileId = fileId;
        }

        public void AttachRiskAnalysis(Guid fileId)
        {
            EnsureDraft();
            RiskAnalysisFileId = fileId;
        }

        public ReviewDecision AddDecision(string description, string responsibleName, DateOnly? dueOn = null)
        {
            EnsureDraft();
            if (string.IsNullOrWhiteSpace(description) || string.IsNullOrWhiteSpace(responsibleName))
                throw new DomainException("REVIEW_DECISION_REQUIRED", "La decisión requiere descripción y responsable.");

            var decision = new ReviewDecision(description.Trim(), responsibleName.Trim(), dueOn);
            _decisions.Add(decision);
            return decision;
        }

        // El seguimiento de decisiones continúa después de aprobada el acta.
        public void CompleteDecision(Guid decisionId, DateOnly completedOn)
        {
            var decision = _decisions.SingleOrDefault(d => d.Id == decisionId)
                ?? throw new DomainException("REVIEW_DECISION_NOT_FOUND", "La decisión no existe.");
            decision.Complete(completedOn);
        }

        public void Approve(ElectronicSignature approvedBy)
        {
            EnsureDraft();
            if (MinutesFileId is null)
                throw new DomainException("REVIEW_MINUTES_REQUIRED", "Adjunta la minuta aprobada.");

            Status = ManagementReviewStatus.Approved;
            ApprovedBy = approvedBy;
        }

        private void EnsureDraft()
        {
            if (Status != ManagementReviewStatus.Draft)
                throw new DomainException("REVIEW_APPROVED", "El acta ya fue aprobada.");
        }
    }
}
