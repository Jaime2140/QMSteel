using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class ListEquipmentsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Equipos";

        public static RouteGroupBuilder MapListEquipments(this RouteGroupBuilder app)
        {
            app.MapGet("equipos", async ([FromQuery(Name = "soloActivos")] bool? onlyActive, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var response = await equipmentService.ListAsync(onlyActive, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<EquipmentSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Tabla de equipos con vigencia de calibración y estatus")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
