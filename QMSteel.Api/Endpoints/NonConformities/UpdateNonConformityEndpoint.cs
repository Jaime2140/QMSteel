using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class UpdateNonConformityEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar NC";

        public static RouteGroupBuilder MapUpdateNonConformity(this RouteGroupBuilder app)
        {
            app.MapPut("no-conformidades/{id:guid}", async (Guid id, [FromBody] UpdateNonConformityRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.UpdateAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Corrige los detalles iniciales de la NC")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
