using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class ReturnEquipmentToServiceEndpoint
    {
        private const string ENDPOINT_NAME = "Regresar Equipo a Servicio";

        public static RouteGroupBuilder MapReturnEquipmentToService(this RouteGroupBuilder app)
        {
            app.MapPost("equipos/{id:guid}/en-servicio", async (Guid id, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var result = await equipmentService.ReturnToServiceAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<EquipmentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Libera el equipo que estaba fuera de servicio")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
