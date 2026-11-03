using QMSteel.Application.Common;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Documents;

namespace QMSteel.Application.Documents
{
    public sealed record CreateDocumentRequest(string Code, string Title, DocumentType Type, int ReviewPeriodMonths,
        string? OwnerArea = null, IReadOnlyList<string>? StandardClauses = null);

    public sealed record UpdateDocumentRequest(string Title, IReadOnlyList<string> StandardClauses);

    public sealed record StartRevisionRequest(Guid FileId, string ChangeDescription);

    public sealed record IssueCopyRequest(string RecipientName, string? RecipientEmail, bool IsExternal, bool AllowDownload,
        bool AllowPrint, DateOnly? ExpiresOn = null);

    public sealed record RevisionDto(Guid Id, int Number, string Label, RevisionStatus Status, Guid FileId, string ChangeDescription,
        SignatureDto PreparedBy, SignatureDto? ReviewedBy, SignatureDto? ApprovedBy, string? RejectionReason,
        DateOnly? EffectiveOn, DateOnly? NextReviewOn)
    {
        public static RevisionDto From(DocumentRevision r) => new(r.Id, r.Number, r.Label, r.Status, r.FileId, r.ChangeDescription,
            SignatureDto.From(r.PreparedBy)!, SignatureDto.From(r.ReviewedBy), SignatureDto.From(r.ApprovedBy), r.RejectionReason,
            r.EffectiveOn, r.NextReviewOn);
    }

    public sealed record CopyDto(Guid Id, Guid RevisionId, string RecipientName, string? RecipientEmail, bool IsExternal,
        bool AllowDownload, bool AllowPrint, DateOnly IssuedOn, DateOnly? ExpiresOn, DateOnly? RevokedOn, bool IsActive)
    {
        public static CopyDto From(ControlledCopy c, DateOnly today) => new(c.Id, c.RevisionId, c.RecipientName, c.RecipientEmail,
            c.IsExternal, c.AllowDownload, c.AllowPrint, c.IssuedOn, c.ExpiresOn, c.RevokedOn, c.IsActiveOn(today));
    }

    public sealed record DocumentSummaryDto(Guid Id, string Code, string Title, DocumentType Type, string? OwnerArea,
        string? CurrentRevision, DateOnly? NextReviewOn, ResourceStatus Status, bool IsActive)
    {
        public static DocumentSummaryDto From(ControlledDocument d, DateOnly today) => new(d.Id, d.Code, d.Title, d.Type, d.OwnerArea,
            d.CurrentRevision?.Label, d.CurrentRevision?.NextReviewOn, d.GetStatus(today), d.IsActive);
    }

    public sealed record DocumentDto(Guid Id, string Code, string Title, DocumentType Type, string? OwnerArea, int ReviewPeriodMonths,
        bool IsActive, ResourceStatus Status, EligibilityDto Eligibility, IReadOnlyList<string> StandardClauses,
        IReadOnlyList<RevisionDto> Revisions, IReadOnlyList<CopyDto> Copies)
    {
        public static DocumentDto From(ControlledDocument d, DateOnly today) => new(d.Id, d.Code, d.Title, d.Type, d.OwnerArea,
            d.ReviewPeriodMonths, d.IsActive, d.GetStatus(today), EligibilityDto.From(d.CheckEligibility(today)),
            d.StandardClauses.ToList(), d.Revisions.OrderByDescending(r => r.Number).Select(RevisionDto.From).ToList(),
            d.Copies.Select(c => CopyDto.From(c, today)).ToList());
    }
}
