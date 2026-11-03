using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Documents
{
    public class DocumentRevision : Entity
    {
        public int Number { get; private set; }
        public string Label => $"Rev. {Number:00}";
        public RevisionStatus Status { get; private set; } = RevisionStatus.Draft;
        public Guid FileId { get; private set; }
        public string ChangeDescription { get; private set; } = default!;
        public ElectronicSignature PreparedBy { get; private set; } = default!;
        public ElectronicSignature? ReviewedBy { get; private set; }
        public ElectronicSignature? ApprovedBy { get; private set; }
        public string? RejectionReason { get; private set; }
        public DateOnly? EffectiveOn { get; private set; }
        public DateOnly? NextReviewOn { get; private set; }

        private DocumentRevision() { }

        internal DocumentRevision(int number, Guid fileId, string changeDescription, ElectronicSignature preparedBy)
        {
            Number = number;
            FileId = fileId;
            ChangeDescription = changeDescription;
            PreparedBy = preparedBy;
        }

        internal void ReplaceFile(Guid fileId) => FileId = fileId;

        internal void Submit(ElectronicSignature reviewedBy)
        {
            Status = RevisionStatus.InReview;
            ReviewedBy = reviewedBy;
            RejectionReason = null;
        }

        internal void Reject(string reason)
        {
            Status = RevisionStatus.Draft;
            ReviewedBy = null;
            RejectionReason = reason;
        }

        internal void Approve(ElectronicSignature approvedBy, DateOnly effectiveOn, DateOnly nextReviewOn)
        {
            Status = RevisionStatus.Effective;
            ApprovedBy = approvedBy;
            EffectiveOn = effectiveOn;
            NextReviewOn = nextReviewOn;
        }

        internal void MarkObsolete() => Status = RevisionStatus.Obsolete;

        internal void ExtendReview(DateOnly nextReviewOn) => NextReviewOn = nextReviewOn;
    }
}
