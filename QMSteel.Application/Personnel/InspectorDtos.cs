using QMSteel.Application.Common;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Welding;

namespace QMSteel.Application.Personnel
{
    public sealed record CreateInspectorRequest(string EmployeeNumber, string FullName, Guid? UserId = null);

    public sealed record AddCertificationRequest(CertificationScheme Scheme, NdtMethod Method, CertificationLevel Level,
        string CertificateNumber, string IssuingBody, DateOnly IssuedOn, DateOnly ExpiresOn, Guid? FileId = null);

    public sealed record RegisterVisionExamRequest(DateOnly ExamDate, bool NearVisionPassed, bool ColorPerceptionPassed,
        string? ExaminerName = null, Guid? FileId = null);

    public sealed record CertificationDto(Guid Id, CertificationScheme Scheme, NdtMethod Method, CertificationLevel Level, string Description,
        string CertificateNumber, string IssuingBody, DateOnly IssuedOn, DateOnly ExpiresOn, Guid? FileId, bool IsValid)
    {
        public static CertificationDto From(InspectorCertification c, DateOnly today) => new(c.Id, c.Scheme, c.Method, c.Level,
            c.Describe(), c.CertificateNumber, c.IssuingBody, c.IssuedOn, c.ExpiresOn, c.FileId, c.IsValidOn(today));
    }

    public sealed record VisionExamDto(Guid Id, DateOnly ExamDate, DateOnly ExpiresOn, bool NearVisionPassed, bool ColorPerceptionPassed,
        bool Passed, string? ExaminerName, Guid? FileId)
    {
        public static VisionExamDto From(VisionExam e) => new(e.Id, e.ExamDate, e.ExpiresOn, e.NearVisionPassed, e.ColorPerceptionPassed,
            e.Passed, e.ExaminerName, e.FileId);
    }

    // Candado operativo por método: indica si hoy puede firmar reportes de ese método NDT.
    public sealed record SigningRightDto(NdtMethod Method, EligibilityDto Eligibility);

    public sealed record InspectorSummaryDto(Guid Id, string EmployeeNumber, string FullName, Guid? UserId, bool IsActive,
        ResourceStatus Status, bool HasValidVisionExam, DateOnly? VisionExamExpiresOn)
    {
        public static InspectorSummaryDto From(Inspector i, DateOnly today) => new(i.Id, i.EmployeeNumber, i.FullName, i.UserId,
            i.IsActive, i.GetStatus(today), i.HasValidVisionExam(today), i.LastVisionExam?.ExpiresOn);
    }

    public sealed record InspectorDto(Guid Id, string EmployeeNumber, string FullName, Guid? UserId, bool IsActive, ResourceStatus Status,
        bool HasValidVisionExam, IReadOnlyList<SigningRightDto> SigningRights, IReadOnlyList<CertificationDto> Certifications,
        IReadOnlyList<VisionExamDto> VisionExams)
    {
        public static InspectorDto From(Inspector i, DateOnly today) => new(i.Id, i.EmployeeNumber, i.FullName, i.UserId, i.IsActive,
            i.GetStatus(today), i.HasValidVisionExam(today),
            Enum.GetValues<NdtMethod>().Select(m => new SigningRightDto(m, EligibilityDto.From(i.CanSign(m, today)))).ToList(),
            i.Certifications.OrderByDescending(c => c.ExpiresOn).Select(c => CertificationDto.From(c, today)).ToList(),
            i.VisionExams.OrderByDescending(e => e.ExamDate).Select(VisionExamDto.From).ToList());
    }
}
