using QMSteel.Application.Documents;
using QMSteel.Application.Equipments;
using QMSteel.Application.Welders;
using QMSteel.Domain.Entities.Documents;
using QMSteel.Domain.Entities.Users;
using QMSteel.Tests.Domain;

namespace QMSteel.Tests.Application
{
    // Reglas que resuelve la capa de aplicación: existencia, duplicados y permisos de firma.
    public class ApplicationRulesTests
    {
        [Fact]
        public async Task Get_UnknownId_ReturnsNotFound()
        {
            using var host = ApplicationHost.Create();

            var result = await host.Get<WelderService>().GetAsync(Guid.NewGuid());

            Assert.Equal("WELDER_NOT_FOUND", result.Error.Code);
        }

        [Fact]
        public async Task CreateWelder_RejectsDuplicateStamp()
        {
            using var host = ApplicationHost.Create();
            await host.Get<WelderService>().CreateAsync(new CreateWelderRequest("E-1", "Soldador Uno", "W-07"));

            var result = await host.Get<WelderService>().CreateAsync(new CreateWelderRequest("E-2", "Soldador Dos", "w-07"));

            Assert.Equal("WELDER_STAMP_DUPLICATE", result.Error.Code);
        }

        [Fact]
        public async Task Auditor_CannotSign()
        {
            using var host = ApplicationHost.Create(UserRole.Auditor);
            var equipment = TestData.ReadyEquipment();
            host.Seed(equipment);

            var result = await host.Get<EquipmentService>().RegisterVerificationAsync(equipment.Id, new RegisterVerificationRequest(true));

            Assert.Equal("USER_READ_ONLY", result.Error.Code);
        }

        [Fact]
        public async Task AreaUser_CannotApproveDocuments()
        {
            using var host = ApplicationHost.Create(UserRole.AreaUser);
            var created = await host.Get<DocumentService>().CreateAsync(new CreateDocumentRequest("PR-01", "Procedimiento de soldadura",
                DocumentType.Procedure, 12));
            await host.Get<DocumentService>().StartRevisionAsync(created.Value.Id, new StartRevisionRequest(Guid.NewGuid(), "Emisión inicial"));
            await host.Get<DocumentService>().SubmitForApprovalAsync(created.Value.Id);

            var result = await host.Get<DocumentService>().ApproveAsync(created.Value.Id);

            Assert.Equal("APPROVER_REQUIRED", result.Error.Code);
        }

        [Fact]
        public async Task QualityManager_ApprovesDocumentWithSignature()
        {
            using var host = ApplicationHost.Create(UserRole.QcInspector);
            var documents = host.Get<DocumentService>();
            var created = await documents.CreateAsync(new CreateDocumentRequest("PR-01", "Procedimiento de soldadura", DocumentType.Procedure, 12));
            await documents.StartRevisionAsync(created.Value.Id, new StartRevisionRequest(Guid.NewGuid(), "Emisión inicial"));
            await documents.SubmitForApprovalAsync(created.Value.Id);
            host.SignInAs(UserRole.QualityManager, "Gerente de Calidad");

            var result = await host.Get<DocumentService>().ApproveAsync(created.Value.Id);

            var revision = Assert.Single(result.Value.Revisions);
            Assert.Equal(RevisionStatus.Effective, revision.Status);
            Assert.Equal("Gerente de Calidad", revision.ApprovedBy?.SignerName);
            Assert.Equal(TestData.Today.AddMonths(12), revision.NextReviewOn);
        }
    }
}
