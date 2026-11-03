using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class CompleteNonConformityActionEndpoint
    {
        private const string ENDPOINT_NAME = "Completar Accion de NC";

        public static RouteGroupBuilder MapCompleteNonConformityAction(this RouteGroupBuilder app)
        {
            app.MapPost("no-conformidades/{id:guid}/acciones/{actionId:guid}/completar", async (Guid id, Guid actionId, [FromBody] CompleteActionRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.CompleteActionAsync(id, actionId, request, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Marca la acción como completada con su evidencia")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
