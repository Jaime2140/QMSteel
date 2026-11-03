using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Suppliers
{
    public enum SupplierCategory { Steel, WeldingConsumables, Coatings, Fasteners, CalibrationServices, NdtServices, Other }

    public enum SupplierStatus { Prospect, Approved, Conditional, Suspended, Removed }

    public enum EvaluationKind { Initial, Periodic }

    public class SupplierEvaluation : Entity
    {
        public EvaluationKind Kind { get; private set; }
        public DateOnly EvaluatedOn { get; private set; }
        public decimal ScorePercent { get; private set; }
        public string? Notes { get; private set; }
        public ElectronicSignature EvaluatedBy { get; private set; } = default!;

        private SupplierEvaluation() { }

        internal SupplierEvaluation(EvaluationKind kind, DateOnly evaluatedOn, decimal scorePercent, string? notes, ElectronicSignature evaluatedBy)
        {
            Kind = kind;
            EvaluatedOn = evaluatedOn;
            ScorePercent = scorePercent;
            Notes = notes;
            EvaluatedBy = evaluatedBy;
        }
    }

    // Proveedor del directorio AVL (Approved Vendor List).
    public class Supplier : AggregateRoot
    {
        public const decimal ApprovalScore = 80m;
        public const decimal ConditionalScore = 60m;

        public string Name { get; private set; } = default!;
        public string? ContactName { get; private set; }
        public string? ContactEmail { get; private set; }
        public string? ContactPhone { get; private set; }
        public SupplierStatus Status { get; private set; } = SupplierStatus.Prospect;
        public string? StatusReason { get; private set; }

        private readonly List<SupplierCategory> _categories = new();
        public IReadOnlyCollection<SupplierCategory> Categories => _categories;

        private readonly List<SupplierEvaluation> _evaluations = new();
        public IReadOnlyCollection<SupplierEvaluation> Evaluations => _evaluations;

        public SupplierEvaluation? LastEvaluation => _evaluations.MaxBy(e => e.EvaluatedOn);
        public decimal? Score => LastEvaluation?.ScorePercent;

        private Supplier() { }

        public Supplier(Guid tenantId, string name, IEnumerable<SupplierCategory> categories,
            string? contactName = null, string? contactEmail = null, string? contactPhone = null)
        {
            TenantId = tenantId;
            UpdateContact(name, contactName, contactEmail, contactPhone);
            SetCategories(categories);
        }

        public void UpdateContact(string name, string? contactName, string? contactEmail, string? contactPhone)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("SUPPLIER_NAME_REQUIRED", "El nombre del proveedor es obligatorio.");

            Name = name.Trim();
            ContactName = contactName?.Trim();
            ContactEmail = contactEmail?.Trim().ToLowerInvariant();
            ContactPhone = contactPhone?.Trim();
        }

        public void SetCategories(IEnumerable<SupplierCategory> categories)
        {
            var list = categories.Distinct().ToList();
            if (list.Count == 0)
                throw new DomainException("SUPPLIER_CATEGORY_REQUIRED", "Indica al menos una categoría del proveedor.");

            _categories.Clear();
            _categories.AddRange(list);
        }

        public bool Supplies(SupplierCategory category) => _categories.Contains(category);

        // La evaluación define el estatus en la AVL salvo que el proveedor esté suspendido o dado de baja.
        public SupplierEvaluation Evaluate(EvaluationKind kind, DateOnly evaluatedOn, decimal scorePercent, ElectronicSignature evaluatedBy, string? notes = null)
        {
            if (Status == SupplierStatus.Removed)
                throw new DomainException("SUPPLIER_REMOVED", "El proveedor está dado de baja.");
            if (scorePercent is < 0 or > 100)
                throw new DomainException("SUPPLIER_SCORE_INVALID", "La calificación debe estar entre 0 y 100.");
            if (kind == EvaluationKind.Initial && _evaluations.Any(e => e.Kind == EvaluationKind.Initial))
                throw new DomainException("SUPPLIER_INITIAL_EVALUATED", "El proveedor ya tiene evaluación inicial.");

            var evaluation = new SupplierEvaluation(kind, evaluatedOn, scorePercent, notes?.Trim(), evaluatedBy);
            _evaluations.Add(evaluation);

            if (Status != SupplierStatus.Suspended)
            {
                Status = scorePercent >= ApprovalScore ? SupplierStatus.Approved
                    : scorePercent >= ConditionalScore ? SupplierStatus.Conditional
                    : _evaluations.Count == 1 ? SupplierStatus.Prospect : SupplierStatus.Suspended;
                StatusReason = Status == SupplierStatus.Suspended ? "Calificación por debajo del mínimo." : null;
            }

            return evaluation;
        }

        public void Suspend(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("SUPPLIER_REASON_REQUIRED", "Indica el motivo.");
            Status = SupplierStatus.Suspended;
            StatusReason = reason.Trim();
        }

        public void Reinstate()
        {
            if (Status != SupplierStatus.Suspended)
                throw new DomainException("SUPPLIER_NOT_SUSPENDED", "El proveedor no está suspendido.");
            Status = Score >= ApprovalScore ? SupplierStatus.Approved : SupplierStatus.Conditional;
            StatusReason = null;
        }

        public void Remove(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("SUPPLIER_REASON_REQUIRED", "Indica el motivo.");
            Status = SupplierStatus.Removed;
            StatusReason = reason.Trim();
        }

        public EligibilityResult CheckEligibility() => Status switch
        {
            SupplierStatus.Approved or SupplierStatus.Conditional => EligibilityResult.Ok(),
            SupplierStatus.Prospect => EligibilityResult.Blocked("SUPPLIER_NOT_APPROVED", "El proveedor no está aprobado en la AVL."),
            SupplierStatus.Suspended => EligibilityResult.Blocked("SUPPLIER_SUSPENDED", $"Proveedor suspendido: {StatusReason}"),
            _ => EligibilityResult.Blocked("SUPPLIER_REMOVED", "El proveedor fue dado de baja de la AVL.")
        };
    }
}
