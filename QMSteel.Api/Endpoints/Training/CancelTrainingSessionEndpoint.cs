using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class CancelTrainingSessionEndpoint
    {
        private const string ENDPOINT_NAME = "Cancelar Sesion de Capacitacion";

        public static RouteGroupBuilder MapCancelTrainingSession(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/sesiones/{id:guid}/cancelar", async (Guid id, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.CancelSessionAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cancela una sesión programada")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
