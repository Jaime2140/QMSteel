using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Standards;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Audits
{
    public class AuditTests
    {
        private static Audit StartedAudit()
        {
            var audit = new Audit(TenantId, "aud-2026-01", AuditType.Internal, QualityStandard.Aisc207, "Capítulo 11", Today, Today.AddDays(2));
            audit.AddParticipant("Auditor Líder", "LIDER@empresa.com", ParticipantRole.LeadAuditor);
            audit.AddParticipant("Jefe de Taller", "taller@empresa.com", ParticipantRole.Auditee);
            audit.Start();
            return audit;
        }

        [Fact]
        public void Schedule_RequiresLeadAuditor()
        {
            var audit = new Audit(TenantId, "AUD-1", AuditType.External, QualityStandard.Iso9001, "Todo el SGC", Today, Today);

            ThrowsDomain(() => audit.Schedule(Now), "AUDIT_LEAD_REQUIRED");
        }

        [Fact]
        public void Schedule_NotifiesAllParticipants()
        {
            var audit = new Audit(TenantId, "AUD-1", AuditType.Internal, QualityStandard.Aisc207, "Cap. 9", Today, Today);
            audit.AddParticipant("Líder", "LIDER@empresa.com", ParticipantRole.LeadAuditor);
            audit.AddParticipant("Almacén", "almacen@empresa.com", ParticipantRole.Auditee);

            audit.Schedule(Now);

            var scheduled = Assert.IsType<AuditScheduled>(Assert.Single(audit.DomainEvents));
            Assert.Equal(new[] { "lider@empresa.com", "almacen@empresa.com" }, scheduled.ParticipantEmails);
        }

        [Theory]
        [InlineData(FindingClassification.MajorNonConformity, true)]
        [InlineData(FindingClassification.MinorNonConformity, true)]
        [InlineData(FindingClassification.Observation, false)]
        [InlineData(FindingClassification.OpportunityForImprovement, false)]
        public void NonConformityFindings_TriggerNc(FindingClassification classification, bool raisesNc)
        {
            var audit = StartedAudit();

            audit.AddFinding("AISC 207 11.3", "Soldador sin continuidad registrada", classification, Now);

            Assert.Equal(raisesNc, audit.DomainEvents.OfType<AuditNonConformityRaised>().Any());
        }

        [Fact]
        public void Findings_OnlyDuringAudit()
        {
            var audit = new Audit(TenantId, "AUD-1", AuditType.Internal, QualityStandard.Aisc207, "Cap. 9", Today, Today);

            ThrowsDomain(() => audit.AddFinding("9.1", "Hallazgo", FindingClassification.Observation, Now), "AUDIT_NOT_IN_PROGRESS");
        }

        [Fact]
        public void OnlyNcFindings_LinkToNonConformity()
        {
            var audit = StartedAudit();
            var observation = audit.AddFinding("9.1", "Mejorar señalización", FindingClassification.Observation, Now);

            ThrowsDomain(() => audit.LinkFindingToNonConformity(observation.Id, Guid.NewGuid()), "AUDIT_FINDING_NOT_NC");
        }

        [Fact]
        public void Complete_ComputesCompliance()
        {
            var audit = StartedAudit();

            audit.Complete(Guid.NewGuid(), requirementsEvaluated: 40, requirementsCompliant: 37);

            Assert.Equal(AuditStatus.Completed, audit.Status);
            Assert.Equal(92.5m, audit.CompliancePercentage);
        }

        [Fact]
        public void InvalidDates_AreRejected() =>
            ThrowsDomain(() => new Audit(TenantId, "AUD-1", AuditType.Internal, QualityStandard.Aisc207, "Cap. 9", Today, Today.AddDays(-1)),
                "AUDIT_INVALID_DATES");

        [Fact]
        public void ManagementReview_RequiresMinutesToApprove()
        {
            var review = new ManagementReview(TenantId, "Revisión anual 2026", Today, new[] { "Director", "Gerente QA" });
            review.AddDecision("Invertir en equipo UT", "Director", Today.AddMonths(3));

            ThrowsDomain(() => review.Approve(Signature()), "REVIEW_MINUTES_REQUIRED");

            review.AttachMinutes(Guid.NewGuid());
            review.Approve(Signature());
            Assert.Equal(ManagementReviewStatus.Approved, review.Status);
            ThrowsDomain(() => review.AddDecision("Otra", "Alguien"), "REVIEW_APPROVED");
        }
    }
}
