using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class UpdateEquipmentEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar Equipo";

        public static RouteGroupBuilder MapUpdateEquipment(this RouteGroupBuilder app)
        {
            app.MapPut("equipos/{id:guid}", async (Guid id, [FromBody] UpdateEquipmentRequest request, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var result = await equipmentService.UpdateAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<EquipmentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cambia la política de calibración, la ubicación y la foto del equipo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
