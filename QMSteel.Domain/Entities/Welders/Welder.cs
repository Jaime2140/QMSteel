using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Welding;

namespace QMSteel.Domain.Entities.Welders
{
    public class Welder : AggregateRoot
    {
        public const int ExpiringSoonDays = 30;

        public string EmployeeNumber { get; private set; } = default!;
        public string FullName { get; private set; } = default!;
        public string StampNumber { get; private set; } = default!;
        public WelderType Type { get; private set; } = WelderType.Welder;
        public bool IsActive { get; private set; } = true;

        private readonly List<WelderQualification> _qualifications = new();
        public IReadOnlyCollection<WelderQualification> Qualifications => _qualifications;

        public DateOnly? LastWeldingActivity { get; private set; }

        private Welder() { }

        public Welder(Guid tenantId, string employeeNumber, string fullName, string stampNumber, WelderType type = WelderType.Welder)
        {
            if (string.IsNullOrWhiteSpace(employeeNumber))
                throw new DomainException("WELDER_EMPLOYEE_REQUIRED", "El número de empleado es obligatorio.");

            TenantId = tenantId;
            EmployeeNumber = employeeNumber.Trim();
            Type = type;
            UpdateInfo(fullName, stampNumber);
        }

        public void UpdateInfo(string fullName, string stampNumber)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("WELDER_NAME_REQUIRED", "El nombre es obligatorio.");
            if (string.IsNullOrWhiteSpace(stampNumber))
                throw new DomainException("WELDER_STAMP_REQUIRED", "La estampa es obligatoria.");

            FullName = fullName.Trim();
            StampNumber = stampNumber.Trim().ToUpperInvariant();
        }

        public void ChangeType(WelderType type) => Type = type;

        public WelderQualification AddQualification(WeldingProcess process, string testPosition, IEnumerable<string> qualifiedPositions,
            DateOnly issuedOn, DateOnly? expiresOn = null, Guid? wpsId = null, Guid? certificateFileId = null)
        {
            var qualification = new WelderQualification(process, testPosition, qualifiedPositions, issuedOn, expiresOn, wpsId, certificateFileId);
            _qualifications.Add(qualification);
            return qualification;
        }

        public void RevokeQualification(Guid qualificationId, string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("QUALIFICATION_REVOCATION_REASON", "Indica el motivo de la revocación.");

            var qualification = _qualifications.SingleOrDefault(q => q.Id == qualificationId)
                ?? throw new DomainException("QUALIFICATION_NOT_FOUND", "La calificación no existe.");
            qualification.Revoke(reason.Trim());
        }

        // La continuidad se mantiene por proceso: soldar con SMAW no mantiene vigente una calificación FCAW.
        public void RegisterWeldingActivity(WeldingProcess process, DateOnly date)
        {
            foreach (var qualification in _qualifications.Where(q => q.Process == process))
                qualification.RegisterUse(date);

            if (LastWeldingActivity is null || date > LastWeldingActivity)
                LastWeldingActivity = date;
        }

        public EligibilityResult CheckEligibility(DateOnly today) => CheckEligibility(today, null, null);

        public EligibilityResult CheckEligibility(DateOnly today, WeldingProcess? process, string? position, bool isTackWeld = false)
        {
            if (!IsActive)
                return EligibilityResult.Blocked("WELDER_INACTIVE", "El soldador está inactivo.");

            if (Type == WelderType.TackWelder && process is not null && !isTackWeld)
                return EligibilityResult.Blocked("WELDER_TACK_ONLY", "El punteador solo puede ejecutar soldaduras de punteo.");

            var covering = _qualifications
                .Where(q => process is null || q.Covers(process.Value, position))
                .ToList();

            if (covering.Count == 0)
                return EligibilityResult.Blocked("WELDER_NOT_QUALIFIED", "El soldador no está calificado para ese proceso y posición.");

            if (covering.Any(q => q.IsValidOn(today)))
                return EligibilityResult.Ok();

            if (covering.Any(q => !q.IsRevoked && q.HasLostContinuityOn(today)))
                return EligibilityResult.Blocked("WELDER_NO_CONTINUITY", "Más de 6 meses sin continuidad de soldadura en el proceso.");

            return EligibilityResult.Blocked("WELDER_EXPIRED", "El soldador no tiene calificación vigente.");
        }

        public ResourceStatus GetStatus(DateOnly today)
        {
            if (!IsActive)
                return ResourceStatus.Blocked;

            var statuses = _qualifications.Select(q => q.GetStatus(today, ExpiringSoonDays)).ToList();

            if (statuses.All(s => s == ResourceStatus.Expired))
                return ResourceStatus.Expired;

            return statuses.Contains(ResourceStatus.Valid) ? ResourceStatus.Valid : ResourceStatus.ExpiringSoon;
        }

        // Alerta roja del boceto: la estampa no ha registrado soldadura en ningún proceso por más de 6 meses.
        public bool HasContinuityAlert(DateOnly today) =>
            IsActive && _qualifications.Count > 0 && _qualifications.All(q => !q.IsValidOn(today))
            && _qualifications.Any(q => !q.IsRevoked && q.HasLostContinuityOn(today));

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
