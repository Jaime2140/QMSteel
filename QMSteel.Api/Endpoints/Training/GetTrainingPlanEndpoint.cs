using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class GetTrainingPlanEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Plan de Capacitacion";

        public static RouteGroupBuilder MapGetTrainingPlan(this RouteGroupBuilder app)
        {
            app.MapGet("capacitacion/planes/{year:int}", async (int year, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.GetPlanAsync(year, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingPlanDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Plan Anual de Capacitación con su cumplimiento y cursos atrasados")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
