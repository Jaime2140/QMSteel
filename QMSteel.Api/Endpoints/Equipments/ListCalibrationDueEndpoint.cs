using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Equipments;
using QMSteel.Domain.Entities.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class ListCalibrationDueEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Calibraciones por Vencer";

        public static RouteGroupBuilder MapListCalibrationDue(this RouteGroupBuilder app)
        {
            app.MapGet("equipos/calibracion-por-vencer", async ([FromQuery(Name = "dias")] int? withinDays, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var response = await equipmentService.ListCalibrationDueAsync(withinDays ?? Equipment.ExpiringSoonDays, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<EquipmentSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Equipos cuya calibración vence dentro de los días indicados (30 por omisión)")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
