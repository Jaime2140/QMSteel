using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Standards;

namespace QMSteel.Application.Audits
{
    public sealed record CreateAuditRequest(AuditType Type, QualityStandard Standard, string Scope, DateOnly PlannedStart, DateOnly PlannedEnd);

    public sealed record AddParticipantRequest(string Name, string Email, ParticipantRole Role, Guid? UserId = null);

    public sealed record RescheduleAuditRequest(DateOnly PlannedStart, DateOnly PlannedEnd);

    public sealed record AddFindingRequest(string Clause, string Description, FindingClassification Classification);

    public sealed record CompleteAuditRequest(Guid ReportFileId, int RequirementsEvaluated, int RequirementsCompliant);

    public sealed record ParticipantDto(Guid Id, string Name, string Email, ParticipantRole Role, Guid? UserId);

    public sealed record FindingDto(Guid Id, int Number, string Clause, string Description, FindingClassification Classification,
        bool IsNonConformity, Guid? NonConformityId);

    // Evento del calendario anual de auditorías.
    public sealed record AuditSummaryDto(Guid Id, string Folio, AuditType Type, QualityStandard Standard, string Scope,
        DateOnly PlannedStart, DateOnly PlannedEnd, AuditStatus Status, decimal? CompliancePercentage)
    {
        public static AuditSummaryDto From(Audit a) => new(a.Id, a.Folio, a.Type, a.Standard, a.Scope, a.PlannedStart, a.PlannedEnd,
            a.Status, a.CompliancePercentage);
    }

    public sealed record AuditDto(Guid Id, string Folio, AuditType Type, QualityStandard Standard, string Scope, DateOnly PlannedStart,
        DateOnly PlannedEnd, AuditStatus Status, Guid? ReportFileId, int? RequirementsEvaluated, int? RequirementsCompliant,
        decimal? CompliancePercentage, IReadOnlyList<ParticipantDto> Participants, IReadOnlyList<FindingDto> Findings)
    {
        public static AuditDto From(Audit a) => new(a.Id, a.Folio, a.Type, a.Standard, a.Scope, a.PlannedStart, a.PlannedEnd, a.Status,
            a.ReportFileId, a.RequirementsEvaluated, a.RequirementsCompliant, a.CompliancePercentage,
            a.Participants.Select(p => new ParticipantDto(p.Id, p.Name, p.Email, p.Role, p.UserId)).ToList(),
            a.Findings.OrderBy(f => f.Number).Select(f => new FindingDto(f.Id, f.Number, f.Clause, f.Description, f.Classification,
                f.IsNonConformity, f.NonConformityId)).ToList());
    }

    public sealed record CreateManagementReviewRequest(string Title, DateOnly HeldOn, IReadOnlyList<string> Attendees);

    public sealed record AddDecisionRequest(string Description, string ResponsibleName, DateOnly? DueOn = null);

    public sealed record DecisionDto(Guid Id, string Description, string ResponsibleName, DateOnly? DueOn, DateOnly? CompletedOn);

    public sealed record ManagementReviewDto(Guid Id, string Title, DateOnly HeldOn, IReadOnlyList<string> Attendees, Guid? MinutesFileId,
        Guid? RiskAnalysisFileId, ManagementReviewStatus Status, SignatureDto? ApprovedBy, IReadOnlyList<DecisionDto> Decisions)
    {
        public static ManagementReviewDto From(ManagementReview r) => new(r.Id, r.Title, r.HeldOn, r.Attendees.ToList(), r.MinutesFileId,
            r.RiskAnalysisFileId, r.Status, SignatureDto.From(r.ApprovedBy),
            r.Decisions.Select(d => new DecisionDto(d.Id, d.Description, d.ResponsibleName, d.DueOn, d.CompletedOn)).ToList());
    }
}
