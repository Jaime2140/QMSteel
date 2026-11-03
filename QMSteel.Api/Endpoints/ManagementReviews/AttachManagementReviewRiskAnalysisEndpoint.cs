using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.ManagementReviews
{
    public static class AttachManagementReviewRiskAnalysisEndpoint
    {
        private const string ENDPOINT_NAME = "Adjuntar Analisis de Riesgos";

        public static RouteGroupBuilder MapAttachManagementReviewRiskAnalysis(this RouteGroupBuilder app)
        {
            app.MapPut("revisiones-direccion/{id:guid}/analisis-riesgos", async (Guid id, [FromBody] FileRequest request, ManagementReviewService managementReviewService, CancellationToken ct) =>
            {
                var result = await managementReviewService.AttachRiskAnalysisAsync(id, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<ManagementReviewDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Adjunta el análisis de riesgos en PDF")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
