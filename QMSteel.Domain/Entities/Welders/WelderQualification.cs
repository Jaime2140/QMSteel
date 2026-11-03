using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Welding;

namespace QMSteel.Domain.Entities.Welders
{
    public class WelderQualification : Entity
    {
        public const int ContinuityMonths = 6;

        public WeldingProcess Process { get; private set; }
        public string TestPosition { get; private set; } = default!;

        // Posiciones que ampara el WPQR, capturadas tal como vienen en el certificado.
        private readonly List<string> _qualifiedPositions = new();
        public IReadOnlyCollection<string> QualifiedPositions => _qualifiedPositions;

        public Guid? WpsId { get; private set; }
        public DateOnly IssuedOn { get; private set; }

        // Null cuando la calificación es indefinida mientras se mantenga la continuidad (AWS D1.1).
        public DateOnly? ExpiresOn { get; private set; }

        public DateOnly? LastUsedOn { get; private set; }
        public Guid? CertificateFileId { get; private set; }
        public string? RevocationReason { get; private set; }
        public bool IsRevoked => RevocationReason is not null;

        private WelderQualification() { }

        internal WelderQualification(WeldingProcess process, string testPosition, IEnumerable<string> qualifiedPositions,
            DateOnly issuedOn, DateOnly? expiresOn, Guid? wpsId, Guid? certificateFileId)
        {
            if (string.IsNullOrWhiteSpace(testPosition))
                throw new DomainException("QUALIFICATION_POSITION_REQUIRED", "La posición de prueba es obligatoria.");
            if (expiresOn is { } exp && exp <= issuedOn)
                throw new DomainException("QUALIFICATION_INVALID_DATES", "La fecha de vencimiento debe ser posterior a la de emisión.");

            Process = process;
            TestPosition = Normalize(testPosition);
            _qualifiedPositions.AddRange(qualifiedPositions.Where(p => !string.IsNullOrWhiteSpace(p)).Select(Normalize).Distinct());
            if (!_qualifiedPositions.Contains(TestPosition))
                _qualifiedPositions.Add(TestPosition);

            IssuedOn = issuedOn;
            ExpiresOn = expiresOn;
            WpsId = wpsId;
            CertificateFileId = certificateFileId;
        }

        public DateOnly ContinuityDeadline => (LastUsedOn ?? IssuedOn).AddMonths(ContinuityMonths);

        public DateOnly EffectiveExpiry => ExpiresOn is { } exp && exp < ContinuityDeadline ? exp : ContinuityDeadline;

        public bool IsValidOn(DateOnly date) => !IsRevoked && date >= IssuedOn && date <= EffectiveExpiry;

        public bool HasLostContinuityOn(DateOnly date) =>
            date > ContinuityDeadline && (ExpiresOn is null || ExpiresOn >= date);

        public bool Covers(WeldingProcess process, string? position) =>
            Process == process && (position is null || _qualifiedPositions.Contains(Normalize(position)));

        public ResourceStatus GetStatus(DateOnly today, int expiringSoonDays)
        {
            if (IsRevoked || !IsValidOn(today))
                return ResourceStatus.Expired;

            return ExpiryAlerts.StatusFor(EffectiveExpiry, today, expiringSoonDays);
        }

        // Una calificación que ya perdió la continuidad no se recupera soldando: requiere recalificar.
        internal void RegisterUse(DateOnly date)
        {
            if (!IsValidOn(date)) return;
            if (LastUsedOn is null || date > LastUsedOn)
                LastUsedOn = date;
        }

        internal void Revoke(string reason) => RevocationReason = reason;

        public void AttachCertificate(Guid fileId) => CertificateFileId = fileId;

        private static string Normalize(string position) => position.Trim().ToUpperInvariant();
    }
}
