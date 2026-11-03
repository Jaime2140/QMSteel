using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class ListSignedInspectionsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Inspecciones Firmadas";

        public static RouteGroupBuilder MapListSignedInspections(this RouteGroupBuilder app)
        {
            app.MapGet("inspecciones/firmadas", async ([FromQuery(Name = "desde")] DateOnly from, [FromQuery(Name = "hasta")] DateOnly to, InspectionService inspectionService, CancellationToken ct) =>
            {
                var response = await inspectionService.ListSignedBetweenAsync(from, to, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<InspectionSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Inspecciones firmadas en el periodo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
