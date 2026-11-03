using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class MarkTrainingSessionHeldEndpoint
    {
        private const string ENDPOINT_NAME = "Marcar Sesion Impartida";

        public static RouteGroupBuilder MapMarkTrainingSessionHeld(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/sesiones/{id:guid}/impartida", async (Guid id, [FromBody] FileRequest request, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.MarkHeldAsync(id, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Marca la sesión como impartida con la lista de asistencia firmada")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
