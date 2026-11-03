using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class ListNonConformitiesEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar NC del Periodo";

        public static RouteGroupBuilder MapListNonConformities(this RouteGroupBuilder app)
        {
            app.MapGet("no-conformidades", async ([FromQuery(Name = "desde")] DateOnly from, [FromQuery(Name = "hasta")] DateOnly to, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var response = await nonConformityService.ListDetectedBetweenAsync(from, to, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<NonConformitySummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("NC detectadas en el periodo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
