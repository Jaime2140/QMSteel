using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class TakeEquipmentOutOfServiceEndpoint
    {
        private const string ENDPOINT_NAME = "Sacar Equipo de Servicio";

        public static RouteGroupBuilder MapTakeEquipmentOutOfService(this RouteGroupBuilder app)
        {
            app.MapPost("equipos/{id:guid}/fuera-de-servicio", async (Guid id, [FromBody] ReasonRequest request, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var result = await equipmentService.TakeOutOfServiceAsync(id, request.Reason, ct);
                return result.ToHttpResult();
            })
            .Produces<EquipmentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Bloquea el uso del equipo indicando el motivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
