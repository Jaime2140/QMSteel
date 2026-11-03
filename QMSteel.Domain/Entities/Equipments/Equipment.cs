using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Equipments
{
    public enum EquipmentCategory { MeasuringInstrument, NdtEquipment, CoatingGauge, WeldingMachine, Other }

    public class Equipment : AggregateRoot
    {
        public const int ExpiringSoonDays = 30;

        public string Code { get; private set; } = default!;
        public string Name { get; private set; } = default!;
        public EquipmentCategory Category { get; private set; }
        public string? Brand { get; private set; }
        public string? Model { get; private set; }
        public string? SerialNumber { get; private set; }
        public string? Location { get; private set; }
        public Guid? PhotoFileId { get; private set; }
        public int CalibrationIntervalMonths { get; private set; }

        // Null cuando el equipo no requiere verificación diaria/periódica.
        public int? VerificationFrequencyDays { get; private set; }

        public bool IsActive { get; private set; } = true;
        public string? OutOfServiceReason { get; private set; }
        public bool IsOutOfService => OutOfServiceReason is not null;

        public int UsageCount { get; private set; }
        public DateOnly? FirstUsedOn { get; private set; }
        public DateOnly? LastUsedOn { get; private set; }

        private readonly List<EquipmentCalibration> _calibrations = new();
        public IReadOnlyCollection<EquipmentCalibration> Calibrations => _calibrations;

        private readonly List<EquipmentVerification> _verifications = new();
        public IReadOnlyCollection<EquipmentVerification> Verifications => _verifications;

        public EquipmentCalibration? LastCalibration => _calibrations.MaxBy(c => c.CalibratedOn);
        public EquipmentVerification? LastVerification => _verifications.MaxBy(v => v.PerformedOn);

        private Equipment() { }

        public Equipment(Guid tenantId, string code, string name, EquipmentCategory category, int calibrationIntervalMonths,
            int? verificationFrequencyDays = null, string? brand = null, string? model = null, string? serialNumber = null)
        {
            if (string.IsNullOrWhiteSpace(code))
                throw new DomainException("EQUIPMENT_CODE_REQUIRED", "El código del equipo es obligatorio.");
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("EQUIPMENT_NAME_REQUIRED", "El nombre del equipo es obligatorio.");

            TenantId = tenantId;
            Code = code.Trim().ToUpperInvariant();
            Name = name.Trim();
            Category = category;
            Brand = brand?.Trim();
            Model = model?.Trim();
            SerialNumber = serialNumber?.Trim();
            SetCalibrationPolicy(calibrationIntervalMonths, verificationFrequencyDays);
        }

        public void SetCalibrationPolicy(int calibrationIntervalMonths, int? verificationFrequencyDays)
        {
            if (calibrationIntervalMonths <= 0)
                throw new DomainException("EQUIPMENT_INTERVAL_INVALID", "El intervalo de calibración debe ser mayor a cero.");
            if (verificationFrequencyDays is <= 0)
                throw new DomainException("EQUIPMENT_VERIFICATION_FREQUENCY_INVALID", "La frecuencia de verificación debe ser mayor a cero.");

            CalibrationIntervalMonths = calibrationIntervalMonths;
            VerificationFrequencyDays = verificationFrequencyDays;
        }

        public void SetLocation(string? location) => Location = location?.Trim();
        public void SetPhoto(Guid? fileId) => PhotoFileId = fileId;

        public EquipmentCalibration RegisterCalibration(DateOnly calibratedOn, string laboratoryName, string? accreditationNumber,
            string certificateNumber, bool passed, Guid? certificateFileId = null, Guid? supplierId = null,
            DateOnly? dueOn = null, DateTime? occurredAtUtc = null)
        {
            if (string.IsNullOrWhiteSpace(laboratoryName))
                throw new DomainException("CALIBRATION_LAB_REQUIRED", "El laboratorio de calibración es obligatorio.");
            if (string.IsNullOrWhiteSpace(certificateNumber))
                throw new DomainException("CALIBRATION_CERTIFICATE_REQUIRED", "El número de certificado es obligatorio.");

            var due = dueOn ?? calibratedOn.AddMonths(CalibrationIntervalMonths);
            if (due <= calibratedOn)
                throw new DomainException("CALIBRATION_INVALID_DATES", "La fecha de vencimiento debe ser posterior a la calibración.");

            var calibration = new EquipmentCalibration(calibratedOn, due, laboratoryName.Trim(), accreditationNumber?.Trim(),
                certificateNumber.Trim(), certificateFileId, supplierId, passed);
            _calibrations.Add(calibration);

            if (!passed)
                Raise(new EquipmentCalibrationFailed(Id, Code, calibratedOn, occurredAtUtc ?? DateTime.UtcNow));

            return calibration;
        }

        public void AttachCalibrationCertificate(Guid calibrationId, Guid fileId)
        {
            var calibration = _calibrations.SingleOrDefault(c => c.Id == calibrationId)
                ?? throw new DomainException("CALIBRATION_NOT_FOUND", "La calibración no existe.");
            calibration.AttachCertificate(fileId);
        }

        public EquipmentVerification RegisterVerification(DateOnly performedOn, bool passed, ElectronicSignature performedBy, string? notes = null)
        {
            if (VerificationFrequencyDays is null)
                throw new DomainException("EQUIPMENT_VERIFICATION_NOT_APPLICABLE", "El equipo no requiere verificación periódica.");
            if (!passed && string.IsNullOrWhiteSpace(notes))
                throw new DomainException("VERIFICATION_NOTES_REQUIRED", "Describe el motivo de la verificación no conforme.");

            var verification = new EquipmentVerification(performedOn, passed, notes?.Trim(), performedBy);
            _verifications.Add(verification);

            if (!passed)
            {
                OutOfServiceReason = $"Verificación no conforme del {performedOn:yyyy-MM-dd}.";
                Raise(new EquipmentVerificationFailed(Id, Code, performedOn, performedBy.SignedAtUtc));
            }

            return verification;
        }

        public void RegisterUse(DateOnly date)
        {
            UsageCount++;
            if (FirstUsedOn is null || date < FirstUsedOn) FirstUsedOn = date;
            if (LastUsedOn is null || date > LastUsedOn) LastUsedOn = date;
        }

        // Promedio de usos por mes desde el primer uso registrado.
        public decimal AverageUsesPerMonth(DateOnly today)
        {
            if (FirstUsedOn is not { } first || UsageCount == 0) return 0;
            var months = Math.Max(1m, (today.DayNumber - first.DayNumber) / 30m);
            return decimal.Round(UsageCount / months, 2);
        }

        public void TakeOutOfService(string reason)
        {
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("EQUIPMENT_OUT_OF_SERVICE_REASON", "Indica el motivo de la baja temporal.");
            OutOfServiceReason = reason.Trim();
        }

        public void ReturnToService() => OutOfServiceReason = null;

        public void Retire() => IsActive = false;

        public ResourceStatus GetStatus(DateOnly today)
        {
            if (!IsActive) return ResourceStatus.Obsolete;
            if (IsOutOfService) return ResourceStatus.Blocked;

            var calibration = LastCalibration;
            if (calibration is null || !calibration.Passed) return ResourceStatus.Blocked;

            return ExpiryAlerts.StatusFor(calibration.DueOn, today, ExpiringSoonDays);
        }

        public bool IsVerificationDue(DateOnly today)
        {
            if (VerificationFrequencyDays is not { } frequency) return false;
            var last = _verifications.Where(v => v.Passed).MaxBy(v => v.PerformedOn);
            return last is null || today.DayNumber - last.PerformedOn.DayNumber >= frequency;
        }

        public EligibilityResult CheckEligibility(DateOnly today)
        {
            switch (GetStatus(today))
            {
                case ResourceStatus.Obsolete:
                    return EligibilityResult.Blocked("EQUIPMENT_RETIRED", "El equipo está dado de baja.");
                case ResourceStatus.Blocked when IsOutOfService:
                    return EligibilityResult.Blocked("EQUIPMENT_OUT_OF_SERVICE", $"Equipo fuera de servicio: {OutOfServiceReason}");
                case ResourceStatus.Blocked:
                    return EligibilityResult.Blocked("EQUIPMENT_NOT_CALIBRATED", "El equipo no tiene una calibración aprobada.");
                case ResourceStatus.Expired:
                    return EligibilityResult.Blocked("EQUIPMENT_CALIBRATION_EXPIRED", "Calibración vencida: equipo bloqueado.");
            }

            if (IsVerificationDue(today))
                return EligibilityResult.Blocked("EQUIPMENT_VERIFICATION_PENDING", "El equipo requiere su verificación diaria/periódica.");

            return EligibilityResult.Ok();
        }
    }
}
