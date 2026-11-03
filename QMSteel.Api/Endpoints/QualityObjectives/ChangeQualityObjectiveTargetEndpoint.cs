using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Compliance;

namespace QMSteel.Api.Endpoints.QualityObjectives
{
    public static class ChangeQualityObjectiveTargetEndpoint
    {
        private const string ENDPOINT_NAME = "Cambiar Meta de Objetivo";

        public static RouteGroupBuilder MapChangeQualityObjectiveTarget(this RouteGroupBuilder app)
        {
            app.MapPut("objetivos/{id:guid}/meta", async (Guid id, [FromBody] TargetRequest request, QualityObjectiveService qualityObjectiveService, CancellationToken ct) =>
            {
                var result = await qualityObjectiveService.ChangeTargetAsync(id, request.Target, ct);
                return result.ToHttpResult();
            })
            .Produces<QualityObjectiveDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cambia la meta del objetivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
