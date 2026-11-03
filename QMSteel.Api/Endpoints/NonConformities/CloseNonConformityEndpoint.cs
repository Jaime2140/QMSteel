using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class CloseNonConformityEndpoint
    {
        private const string ENDPOINT_NAME = "Cerrar NC";

        public static RouteGroupBuilder MapCloseNonConformity(this RouteGroupBuilder app)
        {
            app.MapPost("no-conformidades/{id:guid}/cerrar", async (Guid id, [FromBody] CloseNonConformityRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.CloseAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cierra la NC con firma del gerente e indica si fue eficaz")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
