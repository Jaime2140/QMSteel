using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class CloseTrainingSessionEndpoint
    {
        private const string ENDPOINT_NAME = "Cerrar Sesion de Capacitacion";

        public static RouteGroupBuilder MapCloseTrainingSession(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/sesiones/{id:guid}/cerrar", async (Guid id, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.CloseSessionAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cierra la sesión y da por cumplido el curso del plan")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
