using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class CreateTrainingSessionEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Sesion de Capacitacion";

        public static RouteGroupBuilder MapCreateTrainingSession(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/sesiones", async ([FromBody] CreateSessionRequest request, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.CreateSessionAsync(request, ct);
                return result.ToCreatedResult(trainingSession => $"/api/capacitacion/sesiones/{trainingSession.Id}");
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Programa una sesión, opcionalmente ligada a un curso del plan")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
