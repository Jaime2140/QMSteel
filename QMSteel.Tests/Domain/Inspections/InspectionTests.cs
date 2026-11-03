using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.ValueObjects;
using QMSteel.Domain.Welding;
using static QMSteel.Tests.Domain.TestData;

namespace QMSteel.Tests.Domain.Inspections
{
    public class InspectionTests
    {
        private static Inspection NewInspection(InspectionMethod method, Inspector inspector) =>
            new(TenantId, "ins-001", method, "Obra A", "c-101", Today, inspector.Id, "AWS D1.1 Tabla 8.2");

        [Fact]
        public void SignedWeldInspection_RaisesEventsAndComputesRejection()
        {
            var inspector = CertifiedInspector();
            var welder = QualifiedWelder();
            var equipment = ReadyEquipment();
            var inspection = NewInspection(InspectionMethod.UT, inspector);
            inspection.AssignWps(ApprovedWps());
            inspection.AddEquipment(equipment);
            inspection.AddWeldResult(welder, WeldingProcess.SMAW, "J-1", 1000, 50, "2G", "Falta de fusión");
            inspection.AddWeldResult(welder, WeldingProcess.SMAW, "J-2", 1000, 0, "3G");

            inspection.Sign(inspector, Signature("Inspeccionó"));

            Assert.Equal(InspectionStatus.Signed, inspection.Status);
            Assert.Equal(2.5m, inspection.RejectionRatePercent);
            var signed = Assert.Single(inspection.DomainEvents.OfType<InspectionSigned>());
            Assert.Equal(new WeldActivity(welder.Id, WeldingProcess.SMAW), Assert.Single(signed.WeldActivities));
            Assert.Equal(equipment.Id, Assert.Single(signed.EquipmentIds));
            var rejections = Assert.Single(inspection.DomainEvents.OfType<InspectionRejectionsFound>());
            Assert.Contains("J-1", Assert.Single(rejections.RejectedItems));
        }

        [Fact]
        public void BlocksWelderWithoutContinuity()
        {
            var inspection = NewInspection(InspectionMethod.UT, CertifiedInspector());
            var welder = QualifiedWelder(lastUse: Today.AddMonths(-7));

            ThrowsDomain(() => inspection.AddWeldResult(welder, WeldingProcess.SMAW, "J-1", 500, 0, "3G"), "WELDER_NO_CONTINUITY");
        }

        [Fact]
        public void BlocksWpsInReview()
        {
            var inspection = NewInspection(InspectionMethod.UT, CertifiedInspector());
            var wps = ApprovedWps();
            wps.StartNewRevision();

            ThrowsDomain(() => inspection.AssignWps(wps), "WPS_IN_REVIEW");
        }

        [Fact]
        public void BlocksProcessDifferentFromWps()
        {
            var inspection = NewInspection(InspectionMethod.UT, CertifiedInspector());
            inspection.AssignWps(ApprovedWps(WeldingProcess.FCAW));

            ThrowsDomain(() => inspection.AddWeldResult(QualifiedWelder(), WeldingProcess.SMAW, "J-1", 500, 0, "3G"),
                "INSPECTION_WPS_PROCESS_MISMATCH");
        }

        [Fact]
        public void BlocksUncalibratedEquipment()
        {
            var inspection = NewInspection(InspectionMethod.UT, CertifiedInspector());
            var equipment = ReadyEquipment();
            equipment.TakeOutOfService("Golpe en el transductor");

            ThrowsDomain(() => inspection.AddEquipment(equipment), "EQUIPMENT_OUT_OF_SERVICE");
        }

        [Fact]
        public void Rejection_RequiresDiscontinuityDescription() =>
            ThrowsDomain(() => NewInspection(InspectionMethod.UT, CertifiedInspector())
                .AddWeldResult(QualifiedWelder(), WeldingProcess.SMAW, "J-1", 500, 10, "3G"), "INSPECTION_DISCONTINUITY_REQUIRED");

        [Fact]
        public void Sign_BlockedByVisionLock()
        {
            var inspector = CertifiedInspector();
            var inspection = NewInspection(InspectionMethod.UT, inspector);
            inspection.AddEquipment(ReadyEquipment());
            inspection.AddWeldResult(QualifiedWelder(), WeldingProcess.SMAW, "J-1", 500, 0, "3G");
            inspector.RegisterVisionExam(Today, true, false);

            ThrowsDomain(() => inspection.Sign(inspector, Signature("Inspeccionó")), "INSPECTOR_VISION_EXPIRED");
            Assert.Equal(InspectionStatus.Draft, inspection.Status);
        }

        [Fact]
        public void Sign_OnlyByAssignedInspector()
        {
            var inspection = NewInspection(InspectionMethod.UT, CertifiedInspector());
            inspection.AddEquipment(ReadyEquipment());
            inspection.AddWeldResult(QualifiedWelder(), WeldingProcess.SMAW, "J-1", 500, 0, "3G");

            ThrowsDomain(() => inspection.Sign(CertifiedInspector(), Signature()), "INSPECTION_WRONG_INSPECTOR");
        }

        [Fact]
        public void SignedInspection_IsLocked()
        {
            var inspector = CertifiedInspector();
            var inspection = NewInspection(InspectionMethod.UT, inspector);
            inspection.AddEquipment(ReadyEquipment());
            inspection.AddWeldResult(QualifiedWelder(), WeldingProcess.SMAW, "J-1", 500, 0, "3G");
            inspection.Sign(inspector, Signature());

            ThrowsDomain(() => inspection.SetNotes("cambio"), "INSPECTION_NOT_DRAFT");
        }

        [Fact]
        public void CoatingCapture_NotAllowedOnWeldInspection() =>
            ThrowsDomain(() => NewInspection(InspectionMethod.UT, CertifiedInspector())
                .AddCoatingMeasurement("Patín", 100, 150, new[] { 120m }), "INSPECTION_KIND_MISMATCH");

        [Theory]
        [InlineData(new[] { 110.0, 120.0, 130.0 }, ItemResult.Accepted)]
        [InlineData(new[] { 85.0, 120.0, 130.0 }, ItemResult.Accepted)]
        [InlineData(new[] { 75.0, 140.0, 140.0 }, ItemResult.Rejected)]
        [InlineData(new[] { 90.0, 95.0, 98.0 }, ItemResult.Rejected)]
        [InlineData(new[] { 160.0, 170.0, 150.0 }, ItemResult.Rejected)]
        public void DftMeasurement_UsesAverageAndSpotRule(double[] readings, ItemResult expected)
        {
            var inspection = NewInspection(InspectionMethod.DFT, CertifiedInspector());

            var measurement = inspection.AddCoatingMeasurement("Patín superior", 100, 150, readings.Select(r => (decimal)r));

            Assert.Equal(expected, measurement.Result);
        }

        [Fact]
        public void SurfacePreparation_RequiresCleanlinessStandard()
        {
            var inspector = CertifiedInspector();
            var inspection = NewInspection(InspectionMethod.SurfacePreparation, inspector);
            inspection.AddEquipment(ReadyEquipment());
            inspection.AddCoatingMeasurement("Alma", 50, 75, new[] { 60m, 65m });

            ThrowsDomain(() => inspection.Sign(inspector, Signature()), "INSPECTION_CLEANLINESS_REQUIRED");

            inspection.SetCleanlinessStandard("sspc-sp10");
            inspection.Sign(inspector, Signature());
            Assert.Equal("SSPC-SP10", inspection.CleanlinessStandard);
        }

        [Fact]
        public void Kpis_DirectAcceptanceAndRejection()
        {
            var inspector = CertifiedInspector();
            var inspection = NewInspection(InspectionMethod.UT, inspector);
            inspection.AddEquipment(ReadyEquipment());
            inspection.AddWeldResult(QualifiedWelder(), WeldingProcess.SMAW, "J-1", 800, 200, "3G", "Porosidad");
            inspection.Sign(inspector, Signature());
            var draft = NewInspection(InspectionMethod.UT, inspector);
            draft.AddWeldResult(QualifiedWelder(), WeldingProcess.SMAW, "J-9", 1000, 1000, "3G", "Grieta");

            var all = new[] { inspection, draft };

            Assert.Equal(75m, QMSteel.Domain.Entities.Compliance.QualityKpis.DirectAcceptancePercentage(all));
            Assert.Equal(25m, QMSteel.Domain.Entities.Compliance.QualityKpis.RejectionPercentage(all));
        }
    }
}
