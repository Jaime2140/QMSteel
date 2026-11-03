using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class CreateTrainingPlanEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Plan de Capacitacion";

        public static RouteGroupBuilder MapCreateTrainingPlan(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/planes/{year:int}", async (int year, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.CreatePlanAsync(year, ct);
                return result.ToCreatedResult(trainingPlan => $"/api/capacitacion/planes/{trainingPlan.Year}");
            })
            .Produces<TrainingPlanDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .WithDescription("Crea el Plan Anual de Capacitación del año")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
