using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class ListInspectionsByProjectEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Inspecciones por Obra";

        public static RouteGroupBuilder MapListInspectionsByProject(this RouteGroupBuilder app)
        {
            app.MapGet("inspecciones/por-obra", async ([FromQuery(Name = "obra")] string projectName, InspectionService inspectionService, CancellationToken ct) =>
            {
                var response = await inspectionService.ListByProjectAsync(projectName, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<InspectionSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Inspecciones de una obra, base del dossier de calidad")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
