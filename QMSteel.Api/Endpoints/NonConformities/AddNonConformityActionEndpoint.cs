using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class AddNonConformityActionEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Accion a NC";

        public static RouteGroupBuilder MapAddNonConformityAction(this RouteGroupBuilder app)
        {
            app.MapPost("no-conformidades/{id:guid}/acciones", async (Guid id, [FromBody] AddActionRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.AddActionAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Paso 3: agrega una corrección, acción correctiva o preventiva")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
