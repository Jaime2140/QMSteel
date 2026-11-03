using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Equipments;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Equipments
{
    public class EquipmentTests
    {
        private static Equipment NewEquipment(int? verificationDays = 1) =>
            new(TenantId, "ut-01", "Ultrasonido", EquipmentCategory.NdtEquipment, 12, verificationDays);

        [Fact]
        public void Code_IsNormalized() => Assert.Equal("UT-01", NewEquipment().Code);

        [Fact]
        public void Blocked_WithoutCalibration() =>
            Assert.Equal("EQUIPMENT_NOT_CALIBRATED", NewEquipment().CheckEligibility(Today).ReasonCode);

        [Fact]
        public void Blocked_WhenCalibrationExpired()
        {
            var equipment = NewEquipment(null);
            equipment.RegisterCalibration(Today.AddMonths(-13), "Lab", null, "C-1", true, occurredAtUtc: Now);

            Assert.Equal(ResourceStatus.Expired, equipment.GetStatus(Today));
            Assert.Equal("EQUIPMENT_CALIBRATION_EXPIRED", equipment.CheckEligibility(Today).ReasonCode);
        }

        [Fact]
        public void ExpiringSoon_Within30DaysOfDueDate()
        {
            var equipment = NewEquipment(null);
            equipment.RegisterCalibration(Today.AddMonths(-12).AddDays(20), "Lab", null, "C-1", true, occurredAtUtc: Now);

            Assert.Equal(ResourceStatus.ExpiringSoon, equipment.GetStatus(Today));
            Assert.True(equipment.CheckEligibility(Today).IsEligible);
        }

        [Fact]
        public void FailedCalibration_BlocksAndRaisesEvent()
        {
            var equipment = NewEquipment(null);

            equipment.RegisterCalibration(Today, "Lab", null, "C-1", false, occurredAtUtc: Now);

            Assert.Equal(ResourceStatus.Blocked, equipment.GetStatus(Today));
            Assert.IsType<EquipmentCalibrationFailed>(Assert.Single(equipment.DomainEvents));
        }

        [Fact]
        public void RequiresDailyVerification_BeforeUse()
        {
            var equipment = NewEquipment();
            equipment.RegisterCalibration(Today.AddMonths(-1), "Lab", null, "C-1", true, occurredAtUtc: Now);

            Assert.Equal("EQUIPMENT_VERIFICATION_PENDING", equipment.CheckEligibility(Today).ReasonCode);

            equipment.RegisterVerification(Today, true, Signature("Verificó"));

            Assert.True(equipment.CheckEligibility(Today).IsEligible);
            Assert.Equal("EQUIPMENT_VERIFICATION_PENDING", equipment.CheckEligibility(Today.AddDays(1)).ReasonCode);
        }

        [Fact]
        public void FailedVerification_TakesEquipmentOutOfService()
        {
            var equipment = ReadyEquipment();

            equipment.RegisterVerification(Today, false, Signature("Verificó"), "Bloque patrón fuera de tolerancia");

            Assert.True(equipment.IsOutOfService);
            Assert.Equal("EQUIPMENT_OUT_OF_SERVICE", equipment.CheckEligibility(Today).ReasonCode);
            Assert.Contains(equipment.DomainEvents, e => e is EquipmentVerificationFailed);
        }

        [Fact]
        public void FailedVerification_RequiresNotes() =>
            ThrowsDomain(() => ReadyEquipment().RegisterVerification(Today, false, Signature()), "VERIFICATION_NOTES_REQUIRED");

        [Fact]
        public void Verification_NotApplicable_WhenNoFrequency() =>
            ThrowsDomain(() => NewEquipment(null).RegisterVerification(Today, true, Signature()), "EQUIPMENT_VERIFICATION_NOT_APPLICABLE");

        [Fact]
        public void RetiredEquipment_IsObsolete()
        {
            var equipment = ReadyEquipment();
            equipment.Retire();

            Assert.Equal(ResourceStatus.Obsolete, equipment.GetStatus(Today));
            Assert.Equal("EQUIPMENT_RETIRED", equipment.CheckEligibility(Today).ReasonCode);
        }

        [Fact]
        public void Usage_TracksCountAndAverage()
        {
            var equipment = ReadyEquipment();
            equipment.RegisterUse(Today.AddDays(-60));
            equipment.RegisterUse(Today.AddDays(-30));
            equipment.RegisterUse(Today);

            Assert.Equal(3, equipment.UsageCount);
            Assert.Equal(Today, equipment.LastUsedOn);
            Assert.Equal(1.5m, equipment.AverageUsesPerMonth(Today));
        }
    }
}
