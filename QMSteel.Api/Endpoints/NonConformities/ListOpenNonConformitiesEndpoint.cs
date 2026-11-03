using QMSteel.Api.DTOs;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class ListOpenNonConformitiesEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar NC Abiertas";

        public static RouteGroupBuilder MapListOpenNonConformities(this RouteGroupBuilder app)
        {
            app.MapGet("no-conformidades/abiertas", async (NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var response = await nonConformityService.ListOpenAsync(ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<NonConformitySummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Tabla de desviaciones activas ordenada por criticidad")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
