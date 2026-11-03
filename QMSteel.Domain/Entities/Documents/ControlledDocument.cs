using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Documents
{
    public class ControlledDocument : AggregateRoot
    {
        public const int ExpiringSoonDays = 30;

        public string Code { get; private set; } = default!;
        public string Title { get; private set; } = default!;
        public DocumentType Type { get; private set; }
        public string? OwnerArea { get; private set; }
        public int ReviewPeriodMonths { get; private set; }
        public bool IsActive { get; private set; } = true;

        // Cláusulas de la norma que cubre el documento (ej. "AISC 207 11.1"), base de la matriz de cumplimiento.
        private readonly List<string> _standardClauses = new();
        public IReadOnlyCollection<string> StandardClauses => _standardClauses;

        private readonly List<DocumentRevision> _revisions = new();
        public IReadOnlyCollection<DocumentRevision> Revisions => _revisions;

        private readonly List<ControlledCopy> _copies = new();
        public IReadOnlyCollection<ControlledCopy> Copies => _copies;

        public DocumentRevision? CurrentRevision => _revisions.SingleOrDefault(r => r.Status == RevisionStatus.Effective);

        public DocumentRevision? PendingRevision =>
            _revisions.SingleOrDefault(r => r.Status is RevisionStatus.Draft or RevisionStatus.InReview);

        private ControlledDocument() { }

        public ControlledDocument(Guid tenantId, string code, string title, DocumentType type, int reviewPeriodMonths, string? ownerArea = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("DOCUMENT_CODE_REQUIRED", "El código del documento es obligatorio.");
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("DOCUMENT_TITLE_REQUIRED", "El título del documento es obligatorio.");
            if (reviewPeriodMonths <= 0)
                throw new DomainException("DOCUMENT_REVIEW_PERIOD_INVALID", "El periodo de revisión debe ser mayor a cero.");

            TenantId = tenantId;
            Code = code.Trim().ToUpperInvariant();
            Title = title.Trim();
            Type = type;
            ReviewPeriodMonths = reviewPeriodMonths;
            OwnerArea = ownerArea?.Trim();
        }

        public void Rename(string title)
        {
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("DOCUMENT_TITLE_REQUIRED", "El título del documento es obligatorio.");
            Title = title.Trim();
        }

        public void SetStandardClauses(IEnumerable<string> clauses)
        {
            _standardClauses.Clear();
            _standardClauses.AddRange(clauses.Where(c => !string.IsNullOrWhiteSpace(c)).Select(c => c.Trim()).Distinct());
        }

        public DocumentRevision StartRevision(Guid fileId, string changeDescription, ElectronicSignature preparedBy)
        {
            EnsureActive();
            if (PendingRevision is not null)
                throw new DomainException("DOCUMENT_REVISION_PENDING", "Ya existe una revisión en borrador o en revisión.");
            if (fileId == Guid.Empty)
                throw new DomainException("DOCUMENT_FILE_REQUIRED", "La revisión requiere un archivo.");
            if (string.IsNullOrWhiteSpace(changeDescription))
                throw new DomainException("DOCUMENT_CHANGE_REQUIRED", "Describe el cambio de la revisión.");

            var number = _revisions.Count == 0 ? 0 : _revisions.Max(r => r.Number) + 1;
            var revision = new DocumentRevision(number, fileId, changeDescription.Trim(), preparedBy);
            _revisions.Add(revision);
            return revision;
        }

        public void ReplacePendingFile(Guid fileId)
        {
            var revision = RequirePending(RevisionStatus.Draft);
            revision.ReplaceFile(fileId);
        }

        public void SubmitForApproval(ElectronicSignature reviewedBy) =>
            RequirePending(RevisionStatus.Draft).Submit(reviewedBy);

        public void RejectPending(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("DOCUMENT_REJECTION_REASON_REQUIRED", "Indica el motivo del rechazo.");
            RequirePending(RevisionStatus.InReview).Reject(reason.Trim());
        }

        public void ApprovePending(ElectronicSignature approvedBy, DateOnly effectiveOn)
        {
            var revision = RequirePending(RevisionStatus.InReview);
            if (revision.PreparedBy.UserId == approvedBy.UserId)
                throw new DomainException("DOCUMENT_SELF_APPROVAL", "Quien elabora una revisión no puede aprobarla.");

            var previous = CurrentRevision;
            previous?.MarkObsolete();
            revision.Approve(approvedBy, effectiveOn, effectiveOn.AddMonths(ReviewPeriodMonths));

            Raise(new DocumentRevisionApproved(Id, revision.Id, revision.Number, approvedBy.SignedAtUtc));

            if (previous is not null)
                RevokeCopiesOf(previous.Id, effectiveOn, $"Sustituida por {revision.Label}.", approvedBy.SignedAtUtc);
        }

        // Revisión periódica sin cambios: se confirma la vigencia de la revisión actual.
        public void ConfirmReview(DateOnly reviewedOn)
        {
            var current = CurrentRevision
                ?? throw new DomainException("DOCUMENT_NOT_EFFECTIVE", "El documento no tiene una revisión vigente.");
            current.ExtendReview(reviewedOn.AddMonths(ReviewPeriodMonths));
        }

        public ControlledCopy IssueCopy(string recipientName, string? recipientEmail, bool isExternal,
            bool allowDownload, bool allowPrint, DateOnly issuedOn, DateOnly? expiresOn = null)
        {
            EnsureActive();
            var current = CurrentRevision
                ?? throw new DomainException("DOCUMENT_NOT_EFFECTIVE", "Solo se distribuyen copias de una revisión vigente.");
            if (string.IsNullOrWhiteSpace(recipientName))
                throw new DomainException("COPY_RECIPIENT_REQUIRED", "La copia requiere un destinatario.");
            if (expiresOn is { } exp && exp < issuedOn)
                throw new DomainException("COPY_INVALID_DATES", "La copia no puede vencer antes de emitirse.");

            var copy = new ControlledCopy(current.Id, recipientName.Trim(), recipientEmail?.Trim(), isExternal,
                allowDownload, allowPrint, issuedOn, expiresOn);
            _copies.Add(copy);
            return copy;
        }

        public void RevokeCopy(Guid copyId, DateOnly on, string reason)
        {
            var copy = _copies.SingleOrDefault(c => c.Id == copyId)
                ?? throw new DomainException("COPY_NOT_FOUND", "La copia controlada no existe.");
            copy.Revoke(on, reason);
        }

        public string WatermarkFor(ControlledCopy copy, DateOnly viewedOn)
        {
            var revision = _revisions.Single(r => r.Id == copy.RevisionId);
            return $"COPIA CONTROLADA · {Code} {revision.Label} · {copy.RecipientName} · {viewedOn:yyyy-MM-dd}";
        }

        public ResourceStatus GetStatus(DateOnly today)
        {
            if (!IsActive)
                return ResourceStatus.Obsolete;

            var current = CurrentRevision;
            if (current?.NextReviewOn is not { } nextReview)
                return ResourceStatus.InReview;

            return ExpiryAlerts.StatusFor(nextReview, today, ExpiringSoonDays);
        }

        public EligibilityResult CheckEligibility(DateOnly today) => GetStatus(today) switch
        {
            ResourceStatus.Obsolete => EligibilityResult.Blocked("DOCUMENT_OBSOLETE", "El documento está obsoleto."),
            ResourceStatus.InReview => EligibilityResult.Blocked("DOCUMENT_NOT_EFFECTIVE", "El documento no tiene una revisión vigente."),
            _ => EligibilityResult.Ok()
        };

        public void MakeObsolete(DateOnly on, DateTime occurredAtUtc)
        {
            if (!IsActive) return;
            IsActive = false;
            CurrentRevision?.MarkObsolete();
            foreach (var revisionId in _revisions.Select(r => r.Id))
                RevokeCopiesOf(revisionId, on, "Documento dado de baja.", occurredAtUtc);
        }

        private void RevokeCopiesOf(Guid revisionId, DateOnly on, string reason, DateTime occurredAtUtc)
        {
            var copies = _copies.Where(c => c.RevisionId == revisionId && c.RevokedOn is null).ToList();
            if (copies.Count == 0) return;

            copies.ForEach(c => c.Revoke(on, reason));
            Raise(new ControlledCopiesRevoked(Id, copies.Select(c => c.Id).ToList(), occurredAtUtc));
        }

        private DocumentRevision RequirePending(RevisionStatus expected)
        {
            var revision = PendingRevision;
            if (revision is null || revision.Status != expected)
                throw new DomainException("DOCUMENT_REVISION_STATE", "La revisión no está en el estado requerido para esta acción.");
            return revision;
        }

        private void EnsureActive()
        {
            if (!IsActive)
                throw new DomainException("DOCUMENT_OBSOLETE", "El documento está dado de baja.");
        }
    }
}
