using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Compliance;

namespace QMSteel.Api.Endpoints.QualityObjectives
{
    public static class RecordQualityObjectiveMeasurementEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Medicion de Objetivo";

        public static RouteGroupBuilder MapRecordQualityObjectiveMeasurement(this RouteGroupBuilder app)
        {
            app.MapPost("objetivos/{id:guid}/mediciones", async (Guid id, [FromBody] RecordMeasurementRequest request, QualityObjectiveService qualityObjectiveService, CancellationToken ct) =>
            {
                var result = await qualityObjectiveService.RecordMeasurementAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<QualityObjectiveDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra o corrige la medición mensual del objetivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
