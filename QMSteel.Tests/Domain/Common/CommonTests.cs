using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Entities.Tenants;
using QMSteel.Domain.Entities.Users;
using QMSteel.Domain.ValueObjects;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Common
{
    public class CommonTests
    {
        [Fact]
        public void NumericRange_ContainsRange()
        {
            var range = new NumericRange(100, 140);

            Assert.True(range.Contains(new NumericRange(110, 130)));
            Assert.False(range.Contains(new NumericRange(90, 130)));
            ThrowsDomain(() => new NumericRange(5, 1), "RANGE_INVALID");
        }

        [Fact]
        public void Money_RejectsNegativeAndRounds()
        {
            Assert.Equal(10.13m, new Money(10.125m, Currency.MXN).Amount);
            ThrowsDomain(() => new Money(-1, Currency.USD), "MONEY_NEGATIVE");
        }

        [Fact]
        public void Signature_RequiresUser() =>
            ThrowsDomain(() => new ElectronicSignature(Guid.Empty, "Nombre", "Aprobó", Now), "SIGNATURE_USER_REQUIRED");

        [Theory]
        [InlineData(-1, ResourceStatus.Expired)]
        [InlineData(0, ResourceStatus.ExpiringSoon)]
        [InlineData(30, ResourceStatus.ExpiringSoon)]
        [InlineData(31, ResourceStatus.Valid)]
        public void ExpiryAlerts_Status(int daysUntilExpiry, ResourceStatus expected) =>
            Assert.Equal(expected, ExpiryAlerts.StatusFor(Today.AddDays(daysUntilExpiry), Today, 30));

        [Fact]
        public void Compliance_PartialCountsHalfAndNotApplicableIsExcluded()
        {
            var statuses = new[]
            {
                ComplianceStatus.Compliant, ComplianceStatus.Compliant, ComplianceStatus.PartiallyCompliant,
                ComplianceStatus.NonCompliant, ComplianceStatus.NotApplicable
            };

            Assert.Equal(62.5m, QualityKpis.CompliancePercentage(statuses));
        }

        [Fact]
        public void CalibratedEquipmentPercentage_IgnoresRetired()
        {
            var calibrated = ReadyEquipment("A");
            var uncalibrated = new Equipment(TenantId, "B", "Calibrador", EquipmentCategory.MeasuringInstrument, 12);
            var retired = ReadyEquipment("C");
            retired.Retire();

            Assert.Equal(50m, QualityKpis.CalibratedEquipmentPercentage(new[] { calibrated, uncalibrated, retired }, Today));
        }

        [Fact]
        public void QualityObjective_EvaluatesDirection()
        {
            var rejection = new QualityObjective(TenantId, "Rechazo NDT", "% rechazo UT", "%", 3, TargetDirection.LowerIsBetter, 2026);
            rejection.RecordMeasurement(9, 4.2m);
            rejection.RecordMeasurement(10, 2.1m);
            rejection.RecordMeasurement(10, 2.8m);

            Assert.Equal(2, rejection.Measurements.Count);
            Assert.True(rejection.IsCurrentlyMet);
        }

        [Fact]
        public void Tenant_CannotOperateWhenSuspendedOrExpired()
        {
            var tenant = new Tenant("Fabricante", "Fabricante-MX", BillingCycle.Monthly, Today.AddDays(14));

            Assert.Equal("fabricante-mx", tenant.Slug);
            Assert.True(tenant.CanOperate(Today));
            Assert.False(tenant.CanOperate(Today.AddDays(15)));

            tenant.Activate(Today.AddYears(1));
            tenant.Suspend();
            Assert.False(tenant.CanOperate(Today));
        }

        [Fact]
        public void User_AuditorIsReadOnly()
        {
            var user = new User(TenantId, "idp|1", "Auditor@Empresa.com", "Auditor Externo", UserRole.Auditor);

            Assert.Equal("auditor@empresa.com", user.Email);
            Assert.True(user.IsReadOnly);
            Assert.False(user.CanApprove);
        }
    }
}
