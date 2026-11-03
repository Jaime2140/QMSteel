using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Welding;

namespace QMSteel.Domain.Entities.Personnel
{
    public class Inspector : AggregateRoot
    {
        public const int ExpiringSoonDays = 30;

        public Guid? UserId { get; private set; }
        public string EmployeeNumber { get; private set; } = default!;
        public string FullName { get; private set; } = default!;
        public bool IsActive { get; private set; } = true;

        private readonly List<InspectorCertification> _certifications = new();
        public IReadOnlyCollection<InspectorCertification> Certifications => _certifications;

        private readonly List<VisionExam> _visionExams = new();
        public IReadOnlyCollection<VisionExam> VisionExams => _visionExams;

        public VisionExam? LastVisionExam => _visionExams.MaxBy(e => e.ExamDate);

        private Inspector() { }

        public Inspector(Guid tenantId, string employeeNumber, string fullName, Guid? userId = null)
        {
            if (string.IsNullOrWhiteSpace(employeeNumber))
                throw new DomainException("INSPECTOR_EMPLOYEE_REQUIRED", "El número de empleado es obligatorio.");
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("INSPECTOR_NAME_REQUIRED", "El nombre es obligatorio.");

            TenantId = tenantId;
            EmployeeNumber = employeeNumber.Trim();
            FullName = fullName.Trim();
            UserId = userId;
        }

        public void LinkUser(Guid userId) => UserId = userId;

        public InspectorCertification AddCertification(CertificationScheme scheme, NdtMethod method, CertificationLevel level,
            string certificateNumber, string issuingBody, DateOnly issuedOn, DateOnly expiresOn, Guid? fileId = null)
        {
            if (string.IsNullOrWhiteSpace(certificateNumber))
                throw new DomainException("CERTIFICATION_NUMBER_REQUIRED", "El número de certificado es obligatorio.");
            if (string.IsNullOrWhiteSpace(issuingBody))
                throw new DomainException("CERTIFICATION_ISSUER_REQUIRED", "La entidad emisora es obligatoria.");
            if (expiresOn <= issuedOn)
                throw new DomainException("CERTIFICATION_INVALID_DATES", "La fecha de vencimiento debe ser posterior a la de emisión.");
            if (scheme == CertificationScheme.SntTc1A && level == CertificationLevel.None)
                throw new DomainException("CERTIFICATION_LEVEL_REQUIRED", "Una certificación SNT-TC-1A requiere nivel.");

            var certification = new InspectorCertification(scheme, method, level, certificateNumber.Trim(), issuingBody.Trim(),
                issuedOn, expiresOn, fileId);
            _certifications.Add(certification);
            return certification;
        }

        public VisionExam RegisterVisionExam(DateOnly examDate, bool nearVisionPassed, bool colorPerceptionPassed,
            string? examinerName = null, Guid? fileId = null)
        {
            var exam = new VisionExam(examDate, nearVisionPassed, colorPerceptionPassed, examinerName?.Trim(), fileId);
            _visionExams.Add(exam);
            return exam;
        }

        public bool HasValidVisionExam(DateOnly today) => LastVisionExam?.IsValidOn(today) == true;

        // Candado operativo RF-5: sin examen de la vista vigente no puede firmar reportes NDT.
        public EligibilityResult CanSign(NdtMethod method, DateOnly today)
        {
            if (!IsActive)
                return EligibilityResult.Blocked("INSPECTOR_INACTIVE", "El inspector está inactivo.");
            if (!HasValidVisionExam(today))
                return EligibilityResult.Blocked("INSPECTOR_VISION_EXPIRED", "El examen de la vista del inspector no está vigente.");

            var applicable = _certifications.Where(c => c.AllowsSigning(method)).ToList();
            if (applicable.Count == 0)
                return EligibilityResult.Blocked("INSPECTOR_NOT_CERTIFIED", $"El inspector no está certificado para firmar {method}.");
            if (!applicable.Any(c => c.IsValidOn(today)))
                return EligibilityResult.Blocked("INSPECTOR_CERTIFICATION_EXPIRED", $"La certificación {method} del inspector está vencida.");

            return EligibilityResult.Ok();
        }

        public ResourceStatus GetStatus(DateOnly today)
        {
            if (!IsActive) return ResourceStatus.Blocked;

            var expirations = _certifications.Where(c => c.IsValidOn(today)).Select(c => c.ExpiresOn).ToList();
            if (expirations.Count == 0 || !HasValidVisionExam(today)) return ResourceStatus.Expired;

            expirations.Add(LastVisionExam!.ExpiresOn);
            return ExpiryAlerts.StatusFor(expirations.Min(), today, ExpiringSoonDays);
        }

        // Avisos de 60, 30 y 15 días: solo considera lo vigente más reciente de cada tipo.
        public IReadOnlyList<ExpiryAlert> AlertsDueOn(DateOnly today)
        {
            var alerts = new List<ExpiryAlert>();

            if (LastVisionExam is { } exam && ExpiryAlerts.ThresholdReachedOn(exam.ExpiresOn, today) is { } visionDays)
                alerts.Add(new ExpiryAlert("Examen de la vista", exam.ExpiresOn, visionDays));

            var latestPerCertification = _certifications
                .GroupBy(c => (c.Scheme, c.Method))
                .Select(g => g.MaxBy(c => c.ExpiresOn)!);

            foreach (var certification in latestPerCertification)
            {
                if (ExpiryAlerts.ThresholdReachedOn(certification.ExpiresOn, today) is { } days)
                    alerts.Add(new ExpiryAlert(certification.Describe(), certification.ExpiresOn, days));
            }

            return alerts;
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
