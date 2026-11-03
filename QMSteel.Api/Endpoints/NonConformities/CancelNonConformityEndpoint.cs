using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class CancelNonConformityEndpoint
    {
        private const string ENDPOINT_NAME = "Cancelar NC";

        public static RouteGroupBuilder MapCancelNonConformity(this RouteGroupBuilder app)
        {
            app.MapPost("no-conformidades/{id:guid}/cancelar", async (Guid id, [FromBody] ReasonRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.CancelAsync(id, request.Reason, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cancela la NC indicando el motivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
