using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class GetTrainingSessionEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Sesion de Capacitacion";

        public static RouteGroupBuilder MapGetTrainingSession(this RouteGroupBuilder app)
        {
            app.MapGet("capacitacion/sesiones/{id:guid}", async (Guid id, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.GetSessionAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Detalle de la sesión con asistencia, calificaciones y efectividad")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
