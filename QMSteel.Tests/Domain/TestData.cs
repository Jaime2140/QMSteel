using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Entities.Welders;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;
using QMSteel.Domain.Welding;

namespace QMSteel.Tests.Domain
{
    internal static class TestData
    {
        public static readonly Guid TenantId = Guid.NewGuid();
        public static readonly DateOnly Today = new(2026, 11, 3);
        public static readonly DateTime Now = new(2026, 11, 3, 15, 0, 0, DateTimeKind.Utc);

        public static ElectronicSignature Signature(string meaning = "Aprobó", Guid? userId = null) =>
            new(userId ?? Guid.NewGuid(), "Usuario QA", meaning, Now);

        public static void ThrowsDomain(Action action, string expectedCode)
        {
            var ex = Assert.Throws<DomainException>(action);
            Assert.Equal(expectedCode, ex.Code);
        }

        public static Welder QualifiedWelder(WeldingProcess process = WeldingProcess.SMAW, DateOnly? lastUse = null)
        {
            var used = lastUse ?? Today.AddDays(-10);
            var welder = new Welder(TenantId, "E-001", "Soldador Uno", "W-07");
            welder.AddQualification(process, "3G", new[] { "1G", "2G" }, used.AddMonths(-1));
            welder.RegisterWeldingActivity(process, used);
            return welder;
        }

        public static Equipment ReadyEquipment(string code = "UT-01")
        {
            var equipment = new Equipment(TenantId, code, "Equipo de ultrasonido", EquipmentCategory.NdtEquipment, 12, 1);
            equipment.RegisterCalibration(Today.AddMonths(-2), "Laboratorio EMA", "EMA-123", "CERT-001", true, occurredAtUtc: Now);
            equipment.RegisterVerification(Today, true, Signature("Verificó"));
            return equipment;
        }

        public static Inspector CertifiedInspector(NdtMethod method = NdtMethod.UT)
        {
            var inspector = new Inspector(TenantId, "I-001", "Inspector Uno");
            inspector.AddCertification(CertificationScheme.SntTc1A, method, CertificationLevel.II, "CERT-NDT-1", "Empresa",
                Today.AddYears(-1), Today.AddYears(3));
            inspector.RegisterVisionExam(Today.AddMonths(-3), true, true);
            return inspector;
        }

        public static EssentialVariables Variables(NumericRange? amperage = null, WeldingProcess process = WeldingProcess.SMAW,
            string filler = "E7018", decimal preheat = 10) =>
            new(process, "ASTM A572 Gr50", "II", filler, process == WeldingProcess.GMAW ? "C25" : null, new[] { "3G" },
                new NumericRange(6, 25), preheat, amperage ?? new NumericRange(110, 130), new NumericRange(22, 26),
                new NumericRange(100, 200));

        public static Pqr ApprovedPqr(EssentialVariables? variables = null)
        {
            var pqr = new Pqr(TenantId, "PQR-001", variables ?? Variables(new NumericRange(100, 140)), Today.AddYears(-1),
                fileId: Guid.NewGuid());
            pqr.Approve(Signature());
            return pqr;
        }

        public static Wps ReviewedWps(EssentialVariables? variables = null, bool isPrequalified = false)
        {
            var wps = new Wps(TenantId, "WPS-001", JointType.Butt, WeldType.CompleteJointPenetration, variables ?? Variables(), isPrequalified);
            wps.AttachFile(Guid.NewGuid());
            return wps;
        }

        public static Wps ApprovedWps(WeldingProcess process = WeldingProcess.SMAW)
        {
            var wps = ReviewedWps(Variables(process: process), isPrequalified: true);
            wps.SubmitForReview();
            wps.Approve(Signature(), Today, Array.Empty<Pqr>());
            return wps;
        }
    }
}
