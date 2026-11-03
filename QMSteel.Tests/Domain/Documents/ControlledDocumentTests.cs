using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Documents;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Documents
{
    public class ControlledDocumentTests
    {
        private static readonly Guid AuthorId = Guid.NewGuid();

        private static ControlledDocument EffectiveDocument()
        {
            var document = new ControlledDocument(TenantId, "pr-qa-01", "Control de soldadura", DocumentType.Procedure, 12);
            document.StartRevision(Guid.NewGuid(), "Emisión inicial", Signature("Elaboró", AuthorId));
            document.SubmitForApproval(Signature("Revisó"));
            document.ApprovePending(Signature("Aprobó"), Today);
            return document;
        }

        [Fact]
        public void FirstApproval_IsRev00AndEffective()
        {
            var document = EffectiveDocument();

            Assert.Equal("PR-QA-01", document.Code);
            Assert.Equal("Rev. 00", document.CurrentRevision!.Label);
            Assert.Equal(Today.AddMonths(12), document.CurrentRevision.NextReviewOn);
            Assert.Equal(ResourceStatus.Valid, document.GetStatus(Today));
            Assert.Contains(document.DomainEvents, e => e is DocumentRevisionApproved);
        }

        [Fact]
        public void WithoutEffectiveRevision_IsBlocked()
        {
            var document = new ControlledDocument(TenantId, "PR-1", "Doc", DocumentType.Form, 12);

            Assert.Equal("DOCUMENT_NOT_EFFECTIVE", document.CheckEligibility(Today).ReasonCode);
        }

        [Fact]
        public void Author_CannotApproveOwnRevision()
        {
            var document = new ControlledDocument(TenantId, "PR-1", "Doc", DocumentType.Form, 12);
            document.StartRevision(Guid.NewGuid(), "Inicial", Signature("Elaboró", AuthorId));
            document.SubmitForApproval(Signature("Revisó"));

            ThrowsDomain(() => document.ApprovePending(Signature("Aprobó", AuthorId), Today), "DOCUMENT_SELF_APPROVAL");
        }

        [Fact]
        public void OnlyOnePendingRevision() =>
            ThrowsDomain(() =>
            {
                var document = EffectiveDocument();
                document.StartRevision(Guid.NewGuid(), "Cambio 1", Signature("Elaboró"));
                document.StartRevision(Guid.NewGuid(), "Cambio 2", Signature("Elaboró"));
            }, "DOCUMENT_REVISION_PENDING");

        [Fact]
        public void NewRevision_ObsoletesPreviousAndRevokesItsCopies()
        {
            var document = EffectiveDocument();
            var copy = document.IssueCopy("Cliente X", "cliente@example.com", true, false, false, Today);

            document.StartRevision(Guid.NewGuid(), "Actualiza criterios", Signature("Elaboró", AuthorId));
            document.SubmitForApproval(Signature("Revisó"));
            document.ApprovePending(Signature("Aprobó"), Today.AddDays(1));

            Assert.Equal("Rev. 01", document.CurrentRevision!.Label);
            Assert.Single(document.Revisions, r => r.Status == RevisionStatus.Obsolete);
            Assert.False(copy.IsActiveOn(Today.AddDays(1)));
            Assert.Contains(document.DomainEvents, e => e is ControlledCopiesRevoked);
        }

        [Fact]
        public void RejectedRevision_ReturnsToDraftWithReason()
        {
            var document = EffectiveDocument();
            document.StartRevision(Guid.NewGuid(), "Cambio", Signature("Elaboró"));
            document.SubmitForApproval(Signature("Revisó"));

            document.RejectPending("Falta anexo");

            Assert.Equal(RevisionStatus.Draft, document.PendingRevision!.Status);
            Assert.Equal("Falta anexo", document.PendingRevision.RejectionReason);
            Assert.Equal("Rev. 00", document.CurrentRevision!.Label);
        }

        [Fact]
        public void ReviewOverdue_IsExpiredButStillUsable()
        {
            var document = EffectiveDocument();
            var afterReview = Today.AddMonths(12).AddDays(1);

            Assert.Equal(ResourceStatus.Expired, document.GetStatus(afterReview));
            Assert.True(document.CheckEligibility(afterReview).IsEligible);

            document.ConfirmReview(afterReview);
            Assert.Equal(ResourceStatus.Valid, document.GetStatus(afterReview));
        }

        [Fact]
        public void Watermark_IdentifiesCopy()
        {
            var document = EffectiveDocument();
            var copy = document.IssueCopy("Cliente X", null, true, false, false, Today);

            Assert.Equal("COPIA CONTROLADA · PR-QA-01 Rev. 00 · Cliente X · 2026-11-03", document.WatermarkFor(copy, Today));
        }

        [Fact]
        public void ObsoleteDocument_RevokesAllCopies()
        {
            var document = EffectiveDocument();
            var copy = document.IssueCopy("Planta", null, false, true, true, Today);

            document.MakeObsolete(Today, Now);

            Assert.Equal(ResourceStatus.Obsolete, document.GetStatus(Today));
            Assert.NotNull(copy.RevokedOn);
            ThrowsDomain(() => document.IssueCopy("Otro", null, false, false, false, Today), "DOCUMENT_OBSOLETE");
        }
    }
}
