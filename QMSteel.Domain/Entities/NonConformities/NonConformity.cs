using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.NonConformities
{
    // NC / CAPA en tres pasos: 1. detalles iniciales, 2. análisis de causa raíz (5 porqués), 3. acciones correctivas.
    public class NonConformity : AggregateRoot
    {
        public const int MaxWhys = 5;

        public string Folio { get; private set; } = default!;
        public NonConformitySource Source { get; private set; }
        public Guid? SourceReferenceId { get; private set; }
        public Guid? SupplierId { get; private set; }
        public Criticality Criticality { get; private set; }
        public DefectType DefectType { get; private set; }
        public string? ProjectName { get; private set; }
        public string? ClientName { get; private set; }
        public string? PartNumber { get; private set; }
        public string Description { get; private set; } = default!;
        public DateOnly DetectedOn { get; private set; }
        public Guid ReportedByUserId { get; private set; }
        public NonConformityStatus Status { get; private set; } = NonConformityStatus.Open;
        public Disposition? Disposition { get; private set; }

        private readonly List<Guid> _evidenceFileIds = new();
        public IReadOnlyCollection<Guid> EvidenceFileIds => _evidenceFileIds;

        private readonly List<string> _whys = new();
        public IReadOnlyCollection<string> Whys => _whys;
        public string? RootCause { get; private set; }
        public bool RequiresCorrectiveAction { get; private set; }

        private readonly List<Guid> _committeeUserIds = new();
        public IReadOnlyCollection<Guid> CommitteeUserIds => _committeeUserIds;

        private readonly List<CorrectiveAction> _actions = new();
        public IReadOnlyCollection<CorrectiveAction> Actions => _actions;

        public Money? CostOfPoorQuality { get; private set; }

        public DateOnly? ClosedOn { get; private set; }
        public bool? WasEffective { get; private set; }
        public ElectronicSignature? ClosedBy { get; private set; }
        public string? CancellationReason { get; private set; }

        private NonConformity() { }

        public NonConformity(Guid tenantId, string folio, NonConformitySource source, Criticality criticality, DefectType defectType,
            string description, DateOnly detectedOn, Guid reportedByUserId, DateTime occurredAtUtc,
            Guid? sourceReferenceId = null, Guid? supplierId = null,
            string? projectName = null, string? clientName = null, string? partNumber = null)
        {
            if (string.IsNullOrWhiteSpace(folio))
                throw new DomainException("NC_FOLIO_REQUIRED", "El folio de la NC es obligatorio.");
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("NC_DESCRIPTION_REQUIRED", "Describe la desviación detectada.");

            TenantId = tenantId;
            Folio = folio.Trim().ToUpperInvariant();
            Source = source;
            SourceReferenceId = sourceReferenceId;
            SupplierId = supplierId;
            Criticality = criticality;
            DefectType = defectType;
            Description = description.Trim();
            DetectedOn = detectedOn;
            ReportedByUserId = reportedByUserId;
            ProjectName = projectName?.Trim();
            ClientName = clientName?.Trim();
            PartNumber = partNumber?.Trim();

            Raise(new NonConformityOpened(Id, Folio, criticality, occurredAtUtc));
        }

        // Paso 1
        public void UpdateDetails(Criticality criticality, DefectType defectType, string description,
            string? projectName, string? clientName, string? partNumber)
        {
            EnsureNotFinished();
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("NC_DESCRIPTION_REQUIRED", "Describe la desviación detectada.");

            Criticality = criticality;
            DefectType = defectType;
            Description = description.Trim();
            ProjectName = projectName?.Trim();
            ClientName = clientName?.Trim();
            PartNumber = partNumber?.Trim();
        }

        public void AddEvidence(Guid fileId)
        {
            EnsureNotFinished();
            if (!_evidenceFileIds.Contains(fileId))
                _evidenceFileIds.Add(fileId);
        }

        public void SetDisposition(Disposition disposition)
        {
            EnsureNotFinished();
            Disposition = disposition;
        }

        public void AssignCommittee(IEnumerable<Guid> memberUserIds, DateTime occurredAtUtc)
        {
            EnsureNotFinished();
            var members = memberUserIds.Where(id => id != Guid.Empty).Distinct().ToList();
            if (members.Count == 0)
                throw new DomainException("NC_COMMITTEE_REQUIRED", "La comisión requiere al menos un integrante.");

            _committeeUserIds.Clear();
            _committeeUserIds.AddRange(members);
            Raise(new NonConformityCommitteeAssigned(Id, members, occurredAtUtc));
        }

        // Paso 2
        public void RecordRootCauseAnalysis(IEnumerable<string> whys, string rootCause, bool requiresCorrectiveAction)
        {
            if (Status is not (NonConformityStatus.Open or NonConformityStatus.UnderAnalysis))
                throw new DomainException("NC_ANALYSIS_LOCKED", "El análisis ya no se puede modificar.");

            var whyList = whys.Where(w => !string.IsNullOrWhiteSpace(w)).Select(w => w.Trim()).ToList();
            if (whyList.Count == 0)
                throw new DomainException("NC_WHYS_REQUIRED", "Registra al menos un porqué.");
            if (whyList.Count > MaxWhys)
                throw new DomainException("NC_TOO_MANY_WHYS", $"El análisis admite máximo {MaxWhys} porqués.");
            if (string.IsNullOrWhiteSpace(rootCause))
                throw new DomainException("NC_ROOT_CAUSE_REQUIRED", "Indica la causa raíz.");

            _whys.Clear();
            _whys.AddRange(whyList);
            RootCause = rootCause.Trim();
            RequiresCorrectiveAction = requiresCorrectiveAction;
            Status = NonConformityStatus.UnderAnalysis;
        }

        // Paso 3
        public CorrectiveAction AddAction(ActionType type, string description, string responsibleName, DateOnly dueOn, Guid? responsibleUserId = null)
        {
            if (Status is not (NonConformityStatus.UnderAnalysis or NonConformityStatus.ActionsInProgress or NonConformityStatus.PendingClosure))
                throw new DomainException("NC_ANALYSIS_REQUIRED", "Registra el análisis de causa raíz antes de las acciones.");
            if (string.IsNullOrWhiteSpace(description))
                throw new DomainException("NC_ACTION_DESCRIPTION_REQUIRED", "Describe la acción.");
            if (string.IsNullOrWhiteSpace(responsibleName))
                throw new DomainException("NC_ACTION_RESPONSIBLE_REQUIRED", "La acción requiere un responsable.");

            var action = new CorrectiveAction(type, description.Trim(), responsibleName.Trim(), responsibleUserId, dueOn);
            _actions.Add(action);
            Status = NonConformityStatus.ActionsInProgress;
            return action;
        }

        public void CompleteAction(Guid actionId, DateOnly completedOn, string? notes = null, Guid? evidenceFileId = null)
        {
            EnsureNotFinished();
            FindAction(actionId).Complete(completedOn, notes?.Trim(), evidenceFileId);
            if (_actions.All(a => a.IsCompleted))
                Status = NonConformityStatus.PendingClosure;
        }

        public void RescheduleAction(Guid actionId, DateOnly dueOn)
        {
            EnsureNotFinished();
            FindAction(actionId).Reschedule(dueOn);
        }

        public void RegisterCost(Money cost)
        {
            EnsureNotFinished();
            CostOfPoorQuality = CostOfPoorQuality is null ? cost : CostOfPoorQuality.Add(cost);
        }

        public void Close(ElectronicSignature closedBy, DateOnly closedOn, bool wasEffective)
        {
            if (Status != NonConformityStatus.PendingClosure)
                throw new DomainException("NC_NOT_READY_TO_CLOSE", "La NC requiere análisis y todas sus acciones completadas.");
            if (RequiresCorrectiveAction && !_actions.Any(a => a.Type == ActionType.Corrective))
                throw new DomainException("NC_CORRECTIVE_ACTION_REQUIRED", "La NC fue elevada a acción correctiva y no tiene ninguna.");
            if (Disposition is null && Source is NonConformitySource.Inspection or NonConformitySource.Receiving)
                throw new DomainException("NC_DISPOSITION_REQUIRED", "Indica la disposición del producto no conforme.");

            Status = NonConformityStatus.Closed;
            ClosedOn = closedOn;
            ClosedBy = closedBy;
            WasEffective = wasEffective;
            Raise(new NonConformityClosed(Id, Folio, wasEffective, closedBy.SignedAtUtc));
        }

        public void Cancel(string reason)
        {
            EnsureNotFinished();
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("NC_CANCEL_REASON_REQUIRED", "Indica el motivo de la cancelación.");
            Status = NonConformityStatus.Cancelled;
            CancellationReason = reason.Trim();
        }

        public bool IsOpen => Status is not (NonConformityStatus.Closed or NonConformityStatus.Cancelled);

        public bool IsOverdueOn(DateOnly date) => _actions.Any(a => a.IsOverdueOn(date));

        public int? DaysToClose => ClosedOn is { } closed ? closed.DayNumber - DetectedOn.DayNumber : null;

        private CorrectiveAction FindAction(Guid actionId) => _actions.SingleOrDefault(a => a.Id == actionId)
            ?? throw new DomainException("NC_ACTION_NOT_FOUND", "La acción no existe.");

        private void EnsureNotFinished()
        {
            if (!IsOpen)
                throw new DomainException("NC_FINISHED", "La NC ya está cerrada o cancelada.");
        }
    }
}
