using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Welders;
using QMSteel.Domain.Welding;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Welders
{
    public class WelderTests
    {
        [Fact]
        public void Constructor_RequiresStamp() =>
            ThrowsDomain(() => new Welder(TenantId, "E-1", "Nombre", " "), "WELDER_STAMP_REQUIRED");

        [Fact]
        public void Eligible_ForQualifiedProcessAndPosition()
        {
            var welder = QualifiedWelder();

            Assert.True(welder.CheckEligibility(Today, WeldingProcess.SMAW, "2g").IsEligible);
        }

        [Fact]
        public void Blocked_ForPositionNotCovered()
        {
            var welder = QualifiedWelder();

            var result = welder.CheckEligibility(Today, WeldingProcess.SMAW, "4G");

            Assert.Equal("WELDER_NOT_QUALIFIED", result.ReasonCode);
        }

        [Fact]
        public void Blocked_WhenInactive()
        {
            var welder = QualifiedWelder();
            welder.Deactivate();

            Assert.Equal("WELDER_INACTIVE", welder.CheckEligibility(Today).ReasonCode);
            Assert.Equal(ResourceStatus.Blocked, welder.GetStatus(Today));
        }

        [Fact]
        public void Continuity_IsTrackedPerProcess()
        {
            var welder = new Welder(TenantId, "E-1", "Soldador", "W-1");
            welder.AddQualification(WeldingProcess.SMAW, "3G", Array.Empty<string>(), Today.AddMonths(-7));
            welder.AddQualification(WeldingProcess.FCAW, "3G", Array.Empty<string>(), Today.AddMonths(-7));
            welder.RegisterWeldingActivity(WeldingProcess.SMAW, Today.AddMonths(-2));

            Assert.True(welder.CheckEligibility(Today, WeldingProcess.SMAW, "3G").IsEligible);
            Assert.Equal("WELDER_NO_CONTINUITY", welder.CheckEligibility(Today, WeldingProcess.FCAW, "3G").ReasonCode);
        }

        [Fact]
        public void LostContinuity_IsNotRestoredByWelding()
        {
            var welder = QualifiedWelder(lastUse: Today.AddMonths(-7));

            welder.RegisterWeldingActivity(WeldingProcess.SMAW, Today);

            Assert.Equal("WELDER_NO_CONTINUITY", welder.CheckEligibility(Today, WeldingProcess.SMAW, "3G").ReasonCode);
            Assert.True(welder.HasContinuityAlert(Today));
        }

        [Fact]
        public void Blocked_WhenQualificationExpired()
        {
            var welder = new Welder(TenantId, "E-1", "Soldador", "W-1");
            welder.AddQualification(WeldingProcess.SMAW, "3G", Array.Empty<string>(), Today.AddMonths(-3), Today.AddDays(-1));
            welder.RegisterWeldingActivity(WeldingProcess.SMAW, Today.AddDays(-2));

            Assert.Equal("WELDER_EXPIRED", welder.CheckEligibility(Today, WeldingProcess.SMAW, "3G").ReasonCode);
            Assert.False(welder.HasContinuityAlert(Today));
        }

        [Fact]
        public void TackWelder_OnlyAllowedOnTackWelds()
        {
            var welder = new Welder(TenantId, "E-2", "Punteador", "P-1", WelderType.TackWelder);
            welder.AddQualification(WeldingProcess.SMAW, "3G", Array.Empty<string>(), Today.AddMonths(-1));

            Assert.Equal("WELDER_TACK_ONLY", welder.CheckEligibility(Today, WeldingProcess.SMAW, "3G").ReasonCode);
            Assert.True(welder.CheckEligibility(Today, WeldingProcess.SMAW, "3G", isTackWeld: true).IsEligible);
        }

        [Fact]
        public void RevokedQualification_BlocksWelder()
        {
            var welder = QualifiedWelder();

            welder.RevokeQualification(welder.Qualifications.Single().Id, "Calidad de soldadura cuestionada");

            Assert.Equal("WELDER_EXPIRED", welder.CheckEligibility(Today, WeldingProcess.SMAW, "3G").ReasonCode);
        }

        [Fact]
        public void Status_IsExpiringSoon_WhenContinuityDeadlineWithin30Days()
        {
            var welder = QualifiedWelder(lastUse: Today.AddMonths(-6).AddDays(10));

            Assert.Equal(ResourceStatus.ExpiringSoon, welder.GetStatus(Today));
        }

        [Fact]
        public void Status_IsExpired_WithoutQualifications()
        {
            var welder = new Welder(TenantId, "E-1", "Soldador", "W-1");

            Assert.Equal(ResourceStatus.Expired, welder.GetStatus(Today));
        }

        [Fact]
        public void Qualification_ExpiringBeforeIssue_IsRejected() =>
            ThrowsDomain(() => new Welder(TenantId, "E-1", "Soldador", "W-1")
                .AddQualification(WeldingProcess.SMAW, "3G", Array.Empty<string>(), Today, Today), "QUALIFICATION_INVALID_DATES");

        [Fact]
        public void Qualification_AlwaysCoversItsTestPosition()
        {
            var welder = new Welder(TenantId, "E-1", "Soldador", "W-1");

            var qualification = welder.AddQualification(WeldingProcess.SMAW, "4g", new[] { "1G" }, Today);

            Assert.Contains("4G", qualification.QualifiedPositions);
        }
    }
}
