using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Dashboard;

namespace QMSteel.Api.Endpoints.Dashboard
{
    public static class GetExecutiveDashboardEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Tablero Ejecutivo";

        public static RouteGroupBuilder MapGetExecutiveDashboard(this RouteGroupBuilder app)
        {
            app.MapGet("tablero/ejecutivo", async ([FromQuery(Name = "desde")] DateOnly from, [FromQuery(Name = "hasta")] DateOnly to, DashboardService dashboardService, CancellationToken ct) =>
            {
                var response = await dashboardService.GetExecutiveAsync(from, to, ct);
                return Results.Ok(response);
            })
            .Produces<ExecutiveDashboardDto>(StatusCodes.Status200OK)
            .WithDescription("KPIs de calidad del periodo: cumplimiento, aceptación de soldadura, NC, costo de no calidad y calibración")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
