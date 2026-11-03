using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Compliance;

namespace QMSteel.Api.Endpoints.Compliance
{
    public static class AddComplianceEvidenceEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Evidencia de Cumplimiento";

        public static RouteGroupBuilder MapAddComplianceEvidence(this RouteGroupBuilder app)
        {
            app.MapPost("cumplimiento/requisitos/{requirementId:guid}/evidencias", async (Guid requirementId, [FromBody] FileRequest request, ComplianceService complianceService, CancellationToken ct) =>
            {
                var result = await complianceService.AddEvidenceAsync(requirementId, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<ComplianceRowDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Adjunta evidencia a un requisito ya evaluado")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
