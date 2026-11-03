using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Personnel;

namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class ListInspectorsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Inspectores";

        public static RouteGroupBuilder MapListInspectors(this RouteGroupBuilder app)
        {
            app.MapGet("inspectores", async ([FromQuery(Name = "soloActivos")] bool? onlyActive, InspectorService inspectorService, CancellationToken ct) =>
            {
                var response = await inspectorService.ListAsync(onlyActive, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<InspectorSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Lista de inspectores QC/NDT con estatus y examen de la vista")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
