using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class ListTrainingSessionsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Sesiones de Capacitacion";

        public static RouteGroupBuilder MapListTrainingSessions(this RouteGroupBuilder app)
        {
            app.MapGet("capacitacion/sesiones", async ([FromQuery(Name = "desde")] DateOnly from, [FromQuery(Name = "hasta")] DateOnly to, TrainingService trainingService, CancellationToken ct) =>
            {
                var response = await trainingService.ListSessionsAsync(from, to, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<TrainingSessionDto>>(StatusCodes.Status200OK)
            .WithDescription("Sesiones impartidas o programadas en el periodo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
