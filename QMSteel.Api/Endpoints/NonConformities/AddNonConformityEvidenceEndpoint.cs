using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class AddNonConformityEvidenceEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Evidencia a NC";

        public static RouteGroupBuilder MapAddNonConformityEvidence(this RouteGroupBuilder app)
        {
            app.MapPost("no-conformidades/{id:guid}/evidencias", async (Guid id, [FromBody] FileRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.AddEvidenceAsync(id, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Adjunta una evidencia fotográfica o documental")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
