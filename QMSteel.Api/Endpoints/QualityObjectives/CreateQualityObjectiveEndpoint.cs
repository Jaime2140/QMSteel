using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Compliance;

namespace QMSteel.Api.Endpoints.QualityObjectives
{
    public static class CreateQualityObjectiveEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Objetivo de Calidad";

        public static RouteGroupBuilder MapCreateQualityObjective(this RouteGroupBuilder app)
        {
            app.MapPost("objetivos", async ([FromBody] CreateObjectiveRequest request, QualityObjectiveService qualityObjectiveService, CancellationToken ct) =>
            {
                var result = await qualityObjectiveService.CreateAsync(request, ct);
                return result.ToCreatedResult(qualityObjective => $"/api/objetivos/{qualityObjective.Id}");
            })
            .Produces<QualityObjectiveDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra un objetivo de calidad con su meta")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
