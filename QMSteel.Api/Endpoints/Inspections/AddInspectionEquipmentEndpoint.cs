using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class AddInspectionEquipmentEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Equipo a Inspeccion";

        public static RouteGroupBuilder MapAddInspectionEquipment(this RouteGroupBuilder app)
        {
            app.MapPost("inspecciones/{id:guid}/equipos/{equipmentId:guid}", async (Guid id, Guid equipmentId, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.AddEquipmentAsync(id, equipmentId, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra el equipo utilizado; uno vencido o fuera de servicio se bloquea")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
