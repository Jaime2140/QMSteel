using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Application.NonConformities;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class GetNonConformityMonitorEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Monitor de NC";

        public static RouteGroupBuilder MapGetNonConformityMonitor(this RouteGroupBuilder app)
        {
            app.MapGet("no-conformidades/monitor", async ([FromQuery(Name = "desde")] DateOnly from, [FromQuery(Name = "hasta")] DateOnly to, [FromQuery(Name = "moneda")] Currency? currency, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var response = await nonConformityService.GetMonitorAsync(from, to, currency ?? Currency.USD, ct);
                return Results.Ok(response);
            })
            .Produces<NonConformityMonitorDto>(StatusCodes.Status200OK)
            .WithDescription("NC abiertas, efectividad de cierre y costo de no calidad del periodo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
