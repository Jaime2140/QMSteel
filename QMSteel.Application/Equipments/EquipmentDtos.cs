using QMSteel.Application.Common;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Equipments;

namespace QMSteel.Application.Equipments
{
    public sealed record CreateEquipmentRequest(string Code, string Name, EquipmentCategory Category, int CalibrationIntervalMonths,
        int? VerificationFrequencyDays = null, string? Brand = null, string? Model = null, string? SerialNumber = null,
        string? Location = null);

    public sealed record UpdateEquipmentRequest(int CalibrationIntervalMonths, int? VerificationFrequencyDays, string? Location, Guid? PhotoFileId);

    public sealed record RegisterCalibrationRequest(DateOnly CalibratedOn, string LaboratoryName, string? AccreditationNumber,
        string CertificateNumber, bool Passed, Guid? CertificateFileId = null, Guid? SupplierId = null, DateOnly? DueOn = null);

    public sealed record RegisterVerificationRequest(bool Passed, string? Notes = null, DateOnly? PerformedOn = null);

    public sealed record CalibrationDto(Guid Id, DateOnly CalibratedOn, DateOnly DueOn, string LaboratoryName, string? AccreditationNumber,
        string CertificateNumber, Guid? CertificateFileId, Guid? SupplierId, bool Passed)
    {
        public static CalibrationDto From(EquipmentCalibration c) => new(c.Id, c.CalibratedOn, c.DueOn, c.LaboratoryName,
            c.AccreditationNumber, c.CertificateNumber, c.CertificateFileId, c.SupplierId, c.Passed);
    }

    public sealed record VerificationDto(Guid Id, DateOnly PerformedOn, bool Passed, string? Notes, SignatureDto PerformedBy)
    {
        public static VerificationDto From(EquipmentVerification v) =>
            new(v.Id, v.PerformedOn, v.Passed, v.Notes, SignatureDto.From(v.PerformedBy)!);
    }

    public sealed record EquipmentSummaryDto(Guid Id, string Code, string Name, EquipmentCategory Category, string? Location,
        DateOnly? CalibrationDueOn, bool IsVerificationDue, ResourceStatus Status, EligibilityDto Eligibility, bool IsActive)
    {
        public static EquipmentSummaryDto From(Equipment e, DateOnly today) => new(e.Id, e.Code, e.Name, e.Category, e.Location,
            e.LastCalibration?.DueOn, e.IsVerificationDue(today), e.GetStatus(today), EligibilityDto.From(e.CheckEligibility(today)),
            e.IsActive);
    }

    public sealed record EquipmentDto(Guid Id, string Code, string Name, EquipmentCategory Category, string? Brand, string? Model,
        string? SerialNumber, string? Location, Guid? PhotoFileId, int CalibrationIntervalMonths, int? VerificationFrequencyDays,
        bool IsActive, string? OutOfServiceReason, ResourceStatus Status, EligibilityDto Eligibility, bool IsVerificationDue,
        int UsageCount, DateOnly? LastUsedOn, decimal AverageUsesPerMonth,
        IReadOnlyList<CalibrationDto> Calibrations, IReadOnlyList<VerificationDto> Verifications)
    {
        public static EquipmentDto From(Equipment e, DateOnly today) => new(e.Id, e.Code, e.Name, e.Category, e.Brand, e.Model,
            e.SerialNumber, e.Location, e.PhotoFileId, e.CalibrationIntervalMonths, e.VerificationFrequencyDays, e.IsActive,
            e.OutOfServiceReason, e.GetStatus(today), EligibilityDto.From(e.CheckEligibility(today)), e.IsVerificationDue(today),
            e.UsageCount, e.LastUsedOn, e.AverageUsesPerMonth(today),
            e.Calibrations.OrderByDescending(c => c.CalibratedOn).Select(CalibrationDto.From).ToList(),
            e.Verifications.OrderByDescending(v => v.PerformedOn).Select(VerificationDto.From).ToList());
    }
}
