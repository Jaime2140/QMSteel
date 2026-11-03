using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Standards;

namespace QMSteel.Domain.Entities.Compliance
{
    // Módulo de QMSteel del que sale la evidencia automática de un requisito.
    public enum EvidenceSource { Manual, Documents, Calibration, Welders, Procedures, Inspectors, Training, Suppliers, NonConformities, Audits, Inspections }

    // Catálogo global de requisitos normativos (no pertenece a un tenant); se actualiza con cada edición de la norma.
    public class ComplianceRequirement : AuditableEntity
    {
        public QualityStandard Standard { get; private set; }
        public string Edition { get; private set; } = default!;
        public string Clause { get; private set; } = default!;
        public string Title { get; private set; } = default!;
        public string? Description { get; private set; }
        public EvidenceSource EvidenceSource { get; private set; }
        public bool IsActive { get; private set; } = true;

        private ComplianceRequirement() { }

        public ComplianceRequirement(QualityStandard standard, string edition, string clause, string title,
            EvidenceSource evidenceSource, string? description = null)
        {
            if (string.IsNullOrWhiteSpace(edition))
                throw new DomainException("REQUIREMENT_EDITION_REQUIRED", "Indica la edición de la norma.");
            if (string.IsNullOrWhiteSpace(clause))
                throw new DomainException("REQUIREMENT_CLAUSE_REQUIRED", "La cláusula es obligatoria.");
            if (string.IsNullOrWhiteSpace(title))
                throw new DomainException("REQUIREMENT_TITLE_REQUIRED", "El título del requisito es obligatorio.");

            Standard = standard;
            Edition = edition.Trim();
            Clause = clause.Trim();
            Title = title.Trim();
            EvidenceSource = evidenceSource;
            Description = description?.Trim();
        }

        public void Retire() => IsActive = false;
    }
}
