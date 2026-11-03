using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class CreateEquipmentEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Equipo";

        public static RouteGroupBuilder MapCreateEquipment(this RouteGroupBuilder app)
        {
            app.MapPost("equipos", async ([FromBody] CreateEquipmentRequest request, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var result = await equipmentService.CreateAsync(request, ct);
                return result.ToCreatedResult(equipment => $"/api/equipos/{equipment.Id}");
            })
            .Produces<EquipmentDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Da de alta un equipo de medición o inspección")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
