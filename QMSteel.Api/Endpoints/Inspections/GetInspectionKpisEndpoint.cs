using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class GetInspectionKpisEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Indicadores de Inspeccion";

        public static RouteGroupBuilder MapGetInspectionKpis(this RouteGroupBuilder app)
        {
            app.MapGet("inspecciones/indicadores", async ([FromQuery(Name = "desde")] DateOnly from, [FromQuery(Name = "hasta")] DateOnly to, InspectionService inspectionService, CancellationToken ct) =>
            {
                var response = await inspectionService.GetKpisAsync(from, to, ct);
                return Results.Ok(response);
            })
            .Produces<InspectionKpisDto>(StatusCodes.Status200OK)
            .WithDescription("Tasas de rechazo, metros inspeccionados y tendencia mensual")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
