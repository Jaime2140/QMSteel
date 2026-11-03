using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class EvaluateTrainingEffectivenessEndpoint
    {
        private const string ENDPOINT_NAME = "Evaluar Efectividad";

        public static RouteGroupBuilder MapEvaluateTrainingEffectiveness(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/sesiones/{id:guid}/efectividad", async (Guid id, [FromBody] EvaluateEffectivenessRequest request, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.EvaluateEffectivenessAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra la evaluación de efectividad de un participante")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
