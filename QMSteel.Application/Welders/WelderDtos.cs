using QMSteel.Application.Common;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Entities.Welders;
using QMSteel.Domain.Welding;

namespace QMSteel.Application.Welders
{
    public sealed record CreateWelderRequest(string EmployeeNumber, string FullName, string StampNumber, WelderType Type = WelderType.Welder);

    public sealed record UpdateWelderRequest(string FullName, string StampNumber, WelderType Type);

    public sealed record AddQualificationRequest(WeldingProcess Process, string TestPosition, IReadOnlyList<string> QualifiedPositions,
        DateOnly IssuedOn, DateOnly? ExpiresOn = null, Guid? WpsId = null, Guid? CertificateFileId = null);

    public sealed record QualificationDto(Guid Id, WeldingProcess Process, string TestPosition, IReadOnlyList<string> QualifiedPositions,
        Guid? WpsId, DateOnly IssuedOn, DateOnly? ExpiresOn, DateOnly? LastUsedOn, DateOnly ContinuityDeadline,
        DateOnly EffectiveExpiry, Guid? CertificateFileId, string? RevocationReason, ResourceStatus Status)
    {
        public static QualificationDto From(WelderQualification q, DateOnly today) => new(q.Id, q.Process, q.TestPosition,
            q.QualifiedPositions.ToList(), q.WpsId, q.IssuedOn, q.ExpiresOn, q.LastUsedOn, q.ContinuityDeadline, q.EffectiveExpiry,
            q.CertificateFileId, q.RevocationReason, q.GetStatus(today, Welder.ExpiringSoonDays));
    }

    public sealed record WelderSummaryDto(Guid Id, string EmployeeNumber, string FullName, string StampNumber, WelderType Type,
        bool IsActive, ResourceStatus Status, bool HasContinuityAlert, DateOnly? LastWeldingActivity, IReadOnlyList<WeldingProcess> Processes)
    {
        public static WelderSummaryDto From(Welder w, DateOnly today) => new(w.Id, w.EmployeeNumber, w.FullName, w.StampNumber, w.Type,
            w.IsActive, w.GetStatus(today), w.HasContinuityAlert(today), w.LastWeldingActivity,
            w.Qualifications.Where(q => q.IsValidOn(today)).Select(q => q.Process).Distinct().ToList());
    }

    public sealed record WelderDto(Guid Id, string EmployeeNumber, string FullName, string StampNumber, WelderType Type, bool IsActive,
        ResourceStatus Status, EligibilityDto Eligibility, bool HasContinuityAlert, DateOnly? LastWeldingActivity,
        IReadOnlyList<QualificationDto> Qualifications)
    {
        public static WelderDto From(Welder w, DateOnly today) => new(w.Id, w.EmployeeNumber, w.FullName, w.StampNumber, w.Type,
            w.IsActive, w.GetStatus(today), EligibilityDto.From(w.CheckEligibility(today)), w.HasContinuityAlert(today),
            w.LastWeldingActivity, w.Qualifications.OrderBy(q => q.Process).Select(q => QualificationDto.From(q, today)).ToList());
    }

    // KPIs superiores del módulo RF-3.
    public sealed record WelderKpisDto(int Active, int Valid, int ExpiringSoon, int ExpiredOrInactive);

    // Historial de soldaduras recientes del expediente lateral (vinculado a inspecciones).
    public sealed record WeldHistoryDto(Guid InspectionId, string Folio, DateOnly InspectedOn, InspectionMethod Method,
        InspectionStatus InspectionStatus, string ProjectName, string JointNumber, WeldingProcess Process,
        decimal InspectedLengthMm, decimal RejectedLengthMm, ItemResult Result);
}
