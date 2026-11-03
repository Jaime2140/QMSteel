using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class ListEmployeeTrainingSessionsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Capacitaciones de Empleado";

        public static RouteGroupBuilder MapListEmployeeTrainingSessions(this RouteGroupBuilder app)
        {
            app.MapGet("capacitacion/empleados/{employeeNumber}/sesiones", async (string employeeNumber, TrainingService trainingService, CancellationToken ct) =>
            {
                var response = await trainingService.ListByEmployeeAsync(employeeNumber, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<TrainingSessionDto>>(StatusCodes.Status200OK)
            .WithDescription("Historial de capacitación de un empleado")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
