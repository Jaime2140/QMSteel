using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Compliance;

namespace QMSteel.Api.Endpoints.QualityObjectives
{
    public static class ListQualityObjectivesEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Objetivos de Calidad";

        public static RouteGroupBuilder MapListQualityObjectives(this RouteGroupBuilder app)
        {
            app.MapGet("objetivos", async ([FromQuery(Name = "anio")] int year, QualityObjectiveService qualityObjectiveService, CancellationToken ct) =>
            {
                var response = await qualityObjectiveService.ListByYearAsync(year, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<QualityObjectiveDto>>(StatusCodes.Status200OK)
            .WithDescription("Objetivos de calidad del año con su última medición")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
