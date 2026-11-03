using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Welding;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Personnel
{
    public class InspectorTests
    {
        [Fact]
        public void CanSign_WithValidCertificationAndVisionExam() =>
            Assert.True(CertifiedInspector().CanSign(NdtMethod.UT, Today).IsEligible);

        [Fact]
        public void VisionLock_BlocksSignatureWhenExamExpired()
        {
            var inspector = new Inspector(TenantId, "I-1", "Inspector");
            inspector.AddCertification(CertificationScheme.Cwi, NdtMethod.VT, CertificationLevel.None, "CWI-1", "AWS",
                Today.AddYears(-1), Today.AddYears(2));
            inspector.RegisterVisionExam(Today.AddYears(-1).AddDays(-1), true, true);

            Assert.Equal("INSPECTOR_VISION_EXPIRED", inspector.CanSign(NdtMethod.VT, Today).ReasonCode);
        }

        [Fact]
        public void VisionLock_BlocksSignatureWhenExamFailed()
        {
            var inspector = CertifiedInspector();
            inspector.RegisterVisionExam(Today, nearVisionPassed: true, colorPerceptionPassed: false);

            Assert.Equal("INSPECTOR_VISION_EXPIRED", inspector.CanSign(NdtMethod.UT, Today).ReasonCode);
        }

        [Fact]
        public void LevelI_CannotSign()
        {
            var inspector = new Inspector(TenantId, "I-1", "Inspector");
            inspector.AddCertification(CertificationScheme.SntTc1A, NdtMethod.PT, CertificationLevel.I, "PT-1", "Empresa",
                Today.AddYears(-1), Today.AddYears(2));
            inspector.RegisterVisionExam(Today, true, true);

            Assert.Equal("INSPECTOR_NOT_CERTIFIED", inspector.CanSign(NdtMethod.PT, Today).ReasonCode);
        }

        [Fact]
        public void CannotSign_OtherMethod() =>
            Assert.Equal("INSPECTOR_NOT_CERTIFIED", CertifiedInspector(NdtMethod.UT).CanSign(NdtMethod.RT, Today).ReasonCode);

        [Fact]
        public void CannotSign_WithExpiredCertification()
        {
            var inspector = new Inspector(TenantId, "I-1", "Inspector");
            inspector.AddCertification(CertificationScheme.SntTc1A, NdtMethod.MT, CertificationLevel.II, "MT-1", "Empresa",
                Today.AddYears(-5), Today.AddDays(-1));
            inspector.RegisterVisionExam(Today, true, true);

            Assert.Equal("INSPECTOR_CERTIFICATION_EXPIRED", inspector.CanSign(NdtMethod.MT, Today).ReasonCode);
        }

        [Fact]
        public void SntCertification_RequiresLevel() =>
            ThrowsDomain(() => new Inspector(TenantId, "I-1", "Inspector").AddCertification(CertificationScheme.SntTc1A,
                NdtMethod.UT, CertificationLevel.None, "UT-1", "Empresa", Today, Today.AddYears(1)), "CERTIFICATION_LEVEL_REQUIRED");

        [Theory]
        [InlineData(60)]
        [InlineData(30)]
        [InlineData(15)]
        public void Alerts_OnThresholdDays(int days)
        {
            var inspector = CertifiedInspector();
            var visionExpiry = inspector.LastVisionExam!.ExpiresOn;

            var alert = Assert.Single(inspector.AlertsDueOn(visionExpiry.AddDays(-days)));

            Assert.Equal("Examen de la vista", alert.Item);
            Assert.Equal(days, alert.DaysRemaining);
        }

        [Fact]
        public void NoAlerts_OutsideThresholds()
        {
            var inspector = CertifiedInspector();

            Assert.Empty(inspector.AlertsDueOn(inspector.LastVisionExam!.ExpiresOn.AddDays(-45)));
        }

        [Fact]
        public void Status_ExpiringSoon_WhenVisionExamCloseToExpire()
        {
            var inspector = CertifiedInspector();

            Assert.Equal(ResourceStatus.ExpiringSoon, inspector.GetStatus(inspector.LastVisionExam!.ExpiresOn.AddDays(-10)));
        }
    }
}
