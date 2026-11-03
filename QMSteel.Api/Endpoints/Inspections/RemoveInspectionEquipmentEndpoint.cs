using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class RemoveInspectionEquipmentEndpoint
    {
        private const string ENDPOINT_NAME = "Quitar Equipo de Inspeccion";

        public static RouteGroupBuilder MapRemoveInspectionEquipment(this RouteGroupBuilder app)
        {
            app.MapDelete("inspecciones/{id:guid}/equipos/{equipmentId:guid}", async (Guid id, Guid equipmentId, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.RemoveEquipmentAsync(id, equipmentId, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Quita un equipo del reporte")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
