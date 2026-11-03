using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Dashboard;

namespace QMSteel.Api.Endpoints.Alerts
{
    public static class GetExpiryAlertsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Alertas de Vencimiento";

        public static RouteGroupBuilder MapGetExpiryAlerts(this RouteGroupBuilder app)
        {
            app.MapGet("alertas/vencimientos", async ([FromQuery(Name = "dias")] int? horizonDays, AlertService alertService, CancellationToken ct) =>
            {
                var response = await alertService.GetExpiryAlertsAsync(horizonDays ?? AlertService.DefaultHorizonDays, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<ExpiryAlertDto>>(StatusCodes.Status200OK)
            .WithDescription("Documentos, calibraciones, calificaciones y certificaciones vencidas o por vencer")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
