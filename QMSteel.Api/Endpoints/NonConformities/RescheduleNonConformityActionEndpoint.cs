using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class RescheduleNonConformityActionEndpoint
    {
        private const string ENDPOINT_NAME = "Reprogramar Accion de NC";

        public static RouteGroupBuilder MapRescheduleNonConformityAction(this RouteGroupBuilder app)
        {
            app.MapPut("no-conformidades/{id:guid}/acciones/{actionId:guid}/fecha", async (Guid id, Guid actionId, [FromBody] DateRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.RescheduleActionAsync(id, actionId, request.Date, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cambia la fecha compromiso de la acción")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
