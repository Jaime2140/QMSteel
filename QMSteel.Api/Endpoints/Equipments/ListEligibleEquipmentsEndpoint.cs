using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class ListEligibleEquipmentsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Equipos Elegibles";

        public static RouteGroupBuilder MapListEligibleEquipments(this RouteGroupBuilder app)
        {
            app.MapGet("equipos/elegibles", async (EquipmentService equipmentService, CancellationToken ct) =>
            {
                var response = await equipmentService.ListEligibleAsync(ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<EquipmentSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Equipos que hoy pueden registrarse en una inspección")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
