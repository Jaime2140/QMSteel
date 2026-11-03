using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.ManagementReviews
{
    public static class AddManagementReviewDecisionEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Decision Estrategica";

        public static RouteGroupBuilder MapAddManagementReviewDecision(this RouteGroupBuilder app)
        {
            app.MapPost("revisiones-direccion/{id:guid}/decisiones", async (Guid id, [FromBody] AddDecisionRequest request, ManagementReviewService managementReviewService, CancellationToken ct) =>
            {
                var result = await managementReviewService.AddDecisionAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<ManagementReviewDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra una decisión con su responsable y fecha")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
