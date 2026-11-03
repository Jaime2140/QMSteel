using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.ManagementReviews
{
    public static class CompleteManagementReviewDecisionEndpoint
    {
        private const string ENDPOINT_NAME = "Completar Decision Estrategica";

        public static RouteGroupBuilder MapCompleteManagementReviewDecision(this RouteGroupBuilder app)
        {
            app.MapPost("revisiones-direccion/{id:guid}/decisiones/{decisionId:guid}/completar", async (Guid id, Guid decisionId, [FromBody] OptionalDateRequest request, ManagementReviewService managementReviewService, CancellationToken ct) =>
            {
                var result = await managementReviewService.CompleteDecisionAsync(id, decisionId, request.Date, ct);
                return result.ToHttpResult();
            })
            .Produces<ManagementReviewDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Marca la decisión como cumplida")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
