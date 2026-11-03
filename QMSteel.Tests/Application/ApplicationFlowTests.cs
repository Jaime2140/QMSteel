using QMSteel.Application.Audits;
using QMSteel.Application.Compliance;
using QMSteel.Application.Dashboard;
using QMSteel.Application.Equipments;
using QMSteel.Application.Inspections;
using QMSteel.Application.NonConformities;
using QMSteel.Application.Procedures;
using QMSteel.Application.Suppliers;
using QMSteel.Application.Training;
using QMSteel.Application.Welders;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Entities.Receiving;
using QMSteel.Domain.Entities.Suppliers;
using QMSteel.Domain.Standards;
using QMSteel.Domain.ValueObjects;
using QMSteel.Domain.Welding;
using QMSteel.Tests.Domain;

namespace QMSteel.Tests.Application
{
    // Flujos de punta a punta: casos de uso, manejadores de eventos y persistencia real en SQLite.
    public class ApplicationFlowTests : IDisposable
    {
        private readonly ApplicationHost _host = ApplicationHost.Create();

        public void Dispose() => _host.Dispose();

        [Fact]
        public async Task SigningInspection_RegistersWelderContinuityAndEquipmentUse()
        {
            var welder = TestData.QualifiedWelder();
            var equipment = TestData.ReadyEquipment();
            var wps = TestData.ApprovedWps();
            var inspector = TestData.CertifiedInspector(NdtMethod.UT);
            inspector.LinkUser(_host.User.Id);
            _host.Seed(welder, equipment, wps, inspector);

            var service = _host.Get<InspectionService>();
            var created = await service.CreateAsync(new CreateInspectionRequest(InspectionMethod.UT, "Nave Industrial", "C-12", "AWS D1.1 Tabla 8.2"));
            var id = created.Value.Id;
            Assert.Equal("INS-2026-0001", created.Value.Folio);
            Assert.True((await service.AssignWpsAsync(id, wps.Id)).IsSuccess);
            Assert.True((await service.AddEquipmentAsync(id, equipment.Id)).IsSuccess);
            Assert.True((await service.AddWeldResultAsync(id, new AddWeldResultRequest(welder.Id, WeldingProcess.SMAW, "J-1", 300, 0))).IsSuccess);

            var signed = await _host.Get<InspectionService>().SignAsync(id);

            Assert.True(signed.IsSuccess);
            Assert.Equal(InspectionStatus.Signed, signed.Value.Status);
            var reloadedWelder = (await _host.Get<WelderService>().GetAsync(welder.Id)).Value;
            Assert.Equal(TestData.Today, reloadedWelder.LastWeldingActivity);
            var reloadedEquipment = (await _host.Get<EquipmentService>().GetAsync(equipment.Id)).Value;
            Assert.Equal(1, reloadedEquipment.UsageCount);
            var history = (await _host.Get<WelderService>().GetHistoryAsync(welder.Id)).Value;
            Assert.Equal("J-1", Assert.Single(history).JointNumber);
        }

        [Fact]
        public async Task SigningInspection_RequiresUserToBeInspector()
        {
            var inspector = TestData.CertifiedInspector();
            _host.Seed(inspector);
            var service = _host.Get<InspectionService>();
            var created = await service.CreateAsync(new CreateInspectionRequest(InspectionMethod.VT, "Obra", "C-1", "AWS D1.1",
                InspectorId: inspector.Id));

            var result = await service.SignAsync(created.Value.Id);

            Assert.Equal("USER_NOT_INSPECTOR", result.Error.Code);
        }

        [Fact]
        public async Task MajorAuditFinding_OpensLinkedNonConformity()
        {
            var audits = _host.Get<AuditService>();
            var audit = (await audits.CreateAsync(new CreateAuditRequest(AuditType.Internal, QualityStandard.Aisc207, "Soldadura",
                TestData.Today, TestData.Today.AddDays(1)))).Value;
            await audits.StartAsync(audit.Id);

            var result = await _host.Get<AuditService>().AddFindingAsync(audit.Id,
                new AddFindingRequest("11.1", "Soldador sin continuidad", FindingClassification.MajorNonConformity));

            var nonConformity = Assert.Single(await _host.Get<NonConformityService>().ListOpenAsync());
            Assert.Equal(NonConformitySource.Audit, nonConformity.Source);
            Assert.Equal(Criticality.High, nonConformity.Criticality);
            Assert.Equal("NC-2026-0001", nonConformity.Folio);
            Assert.Equal(nonConformity.Id, Assert.Single(result.Value.Findings).NonConformityId);
            // El resultado del caso de uso ya refleja la liga porque el manejador corre antes de guardar.
            Assert.Equal(nonConformity.Id, Assert.Single((await _host.Get<AuditService>().GetAsync(audit.Id)).Value.Findings).NonConformityId);
        }

        [Fact]
        public async Task RejectedReceiving_OpensNonConformityForSupplier()
        {
            var suppliers = _host.Get<SupplierService>();
            var supplier = (await suppliers.CreateAsync(new CreateSupplierRequest("Aceros del Norte", [SupplierCategory.Steel]))).Value;
            await suppliers.EvaluateAsync(supplier.Id, new EvaluateSupplierRequest(EvaluationKind.Initial, 92));

            var receivings = _host.Get<ReceivingService>();
            var receiving = (await receivings.StartAsync(new StartReceivingRequest(supplier.Id, "OC-100", "REM-55"))).Value;
            receiving = (await receivings.AddItemAsync(receiving.Id, new AddReceivedItemRequest(MaterialType.Steel, "Placa A572", 10, "pza",
                HeatNumber: "H-778", MtrStatus: MtrStatus.Complete, MtrFileId: Guid.NewGuid()))).Value;
            await receivings.RecordVisualInspectionAsync(receiving.Id, receiving.Items.Single().Id,
                new VisualInspectionRequest(false, [ReceivingDefect.Oxidation]));

            var completed = await _host.Get<ReceivingService>().CompleteAsync(receiving.Id);

            Assert.Equal(ReceivingStatus.Rejected, completed.Value.Status);
            var nonConformity = Assert.Single(await _host.Get<NonConformityService>().ListOpenAsync());
            Assert.Equal(NonConformitySource.Receiving, nonConformity.Source);
            Assert.Equal(supplier.Id, (await _host.Get<NonConformityService>().GetAsync(nonConformity.Id)).Value.SupplierId);
            Assert.Single(await _host.Get<ReceivingService>().ListByHeatNumberAsync("h-778"));
        }

        [Fact]
        public async Task ApprovingWps_FailsWhenPqrDoesNotSupportRanges()
        {
            var procedures = _host.Get<ProcedureService>();
            var pqr = (await procedures.CreatePqrAsync(new CreatePqrRequest("PQR-01",
                EssentialVariablesDto.From(TestData.Variables(new NumericRange(100, 120))), TestData.Today.AddYears(-1),
                FileId: Guid.NewGuid()))).Value;
            await procedures.ApprovePqrAsync(pqr.Id);
            var wps = (await procedures.CreateWpsAsync(new CreateWpsRequest("WPS-01", JointType.Butt, WeldType.CompleteJointPenetration,
                EssentialVariablesDto.From(TestData.Variables(new NumericRange(110, 130))), false, Guid.NewGuid()))).Value;
            await procedures.LinkPqrAsync(wps.Id, pqr.Id);
            await procedures.SubmitForReviewAsync(wps.Id);

            var result = await _host.Get<ProcedureService>().ApproveWpsAsync(wps.Id);

            Assert.Equal("WPS_PQR_DISCREPANCIES", result.Error.Code);
            Assert.Contains("Amperaje", result.Error.Message);
            Assert.Equal(WpsStatus.InReview, (await _host.Get<ProcedureService>().GetWpsAsync(wps.Id)).Value.Status);
            Assert.NotEmpty(Assert.Single(await _host.Get<ProcedureService>().GetMatrixAsync()).Discrepancies);
        }

        [Fact]
        public async Task ClosingPlannedSession_CompletesCourseAndIssuesCertificate()
        {
            var training = _host.Get<TrainingService>();
            var plan = (await training.CreatePlanAsync(2026)).Value;
            plan = (await training.AddCourseAsync(plan.Id, new AddCourseRequest("Inspección visual", 11, true))).Value;
            var session = (await training.CreateSessionAsync(new CreateSessionRequest("Inspección visual", "Instructor QA",
                TestData.Today, 8, PlannedCourseId: plan.Courses.Single().Id))).Value;
            await training.AddAttendeeAsync(session.Id, new AttendeeRequest("E-001", "Soldador Uno"));
            await training.RecordAttendanceAsync(session.Id, new RecordAttendanceRequest("E-001", true));
            await training.MarkHeldAsync(session.Id, Guid.NewGuid());
            await training.EvaluateEffectivenessAsync(session.Id, new EvaluateEffectivenessRequest("E-001", true));

            await _host.Get<TrainingService>().CloseSessionAsync(session.Id);
            var certified = await _host.Get<TrainingService>().IssueCertificateAsync(session.Id, "E-001");

            Assert.Equal("CONST-2026-0001", Assert.Single(certified.Value.Attendees).CertificateFolio);
            Assert.Equal(100, (await _host.Get<TrainingService>().GetPlanAsync(2026)).Value.CompliancePercentage);
        }

        [Fact]
        public async Task ComplianceMatrix_CountsOnlyAssessedRequirements()
        {
            var compliant = new ComplianceRequirement(QualityStandard.Aisc207, "2020", "11.1", "Calificación de soldadores", EvidenceSource.Welders);
            var partial = new ComplianceRequirement(QualityStandard.Aisc207, "2020", "11.2", "Inspectores", EvidenceSource.Inspectors);
            var pending = new ComplianceRequirement(QualityStandard.Aisc207, "2020", "9.1", "Recepción", EvidenceSource.Suppliers);
            _host.Seed(compliant, partial, pending);
            var compliance = _host.Get<ComplianceService>();
            await compliance.AssessAsync(compliant.Id, new AssessRequirementRequest(ComplianceStatus.Compliant));
            await compliance.AssessAsync(partial.Id, new AssessRequirementRequest(ComplianceStatus.PartiallyCompliant));

            var matrix = await _host.Get<ComplianceService>().GetMatrixAsync(QualityStandard.Aisc207);
            var chapter11 = await _host.Get<ComplianceService>().GetMatrixAsync(QualityStandard.Aisc207, "11");

            Assert.Equal(75, matrix.CompliancePercentage);
            Assert.Equal(1, matrix.Pending);
            Assert.Equal(2, chapter11.Requirements);
        }

        [Fact]
        public async Task Alerts_IncludeCalibrationAboutToExpire()
        {
            var equipment = new Equipment(TestData.TenantId, "PT-01", "Kit de líquidos", EquipmentCategory.NdtEquipment, 12);
            equipment.RegisterCalibration(TestData.Today.AddMonths(-12).AddDays(20), "Laboratorio EMA", "EMA-1", "CERT-9", true);
            _host.Seed(equipment);

            var alerts = await _host.Get<AlertService>().GetExpiryAlertsAsync();

            var alert = Assert.Single(alerts);
            Assert.Equal(AlertModule.Equipments, alert.Module);
            Assert.Equal(20, alert.DaysRemaining);
            Assert.Equal(ResourceStatus.ExpiringSoon, alert.Status);
        }

        // Ejecuta las consultas de listados y tableros para detectar las que SQLite no puede traducir.
        [Fact]
        public async Task ListingsAndDashboards_RunOnEmptyDatabase()
        {
            var from = TestData.Today.AddMonths(-1);

            await _host.Get<DashboardService>().GetExecutiveAsync(from, TestData.Today);
            await _host.Get<InspectionService>().GetKpisAsync(from, TestData.Today);
            await _host.Get<NonConformityService>().GetMonitorAsync(from, TestData.Today, Currency.MXN);
            await _host.Get<WelderService>().GetKpisAsync();
            await _host.Get<WelderService>().ListEligibleAsync(WeldingProcess.SMAW, "1G");
            await _host.Get<EquipmentService>().ListCalibrationDueAsync();
            await _host.Get<ProcedureService>().ListWpsAsync(WeldingProcess.FCAW, WpsStatus.Approved);
            await _host.Get<SupplierService>().ListAsync(SupplierStatus.Approved, SupplierCategory.Steel);
            await _host.Get<AuditService>().ListByYearAsync(2026);
            await _host.Get<ManagementReviewService>().ListByYearAsync(2026);
            await _host.Get<TrainingService>().ListByEmployeeAsync("E-001");
            await _host.Get<QualityObjectiveService>().ListByYearAsync(2026);
        }
    }
}
