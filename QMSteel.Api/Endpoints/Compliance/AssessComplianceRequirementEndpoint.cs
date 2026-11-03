using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Compliance;

namespace QMSteel.Api.Endpoints.Compliance
{
    public static class AssessComplianceRequirementEndpoint
    {
        private const string ENDPOINT_NAME = "Evaluar Requisito";

        public static RouteGroupBuilder MapAssessComplianceRequirement(this RouteGroupBuilder app)
        {
            app.MapPut("cumplimiento/requisitos/{requirementId:guid}/evaluacion", async (Guid requirementId, [FromBody] AssessRequirementRequest request, ComplianceService complianceService, CancellationToken ct) =>
            {
                var result = await complianceService.AssessAsync(requirementId, request, ct);
                return result.ToHttpResult();
            })
            .Produces<ComplianceRowDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra el estatus de cumplimiento de un requisito")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
