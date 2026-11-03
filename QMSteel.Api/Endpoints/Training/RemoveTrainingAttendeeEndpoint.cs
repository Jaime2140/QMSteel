using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class RemoveTrainingAttendeeEndpoint
    {
        private const string ENDPOINT_NAME = "Quitar Participante";

        public static RouteGroupBuilder MapRemoveTrainingAttendee(this RouteGroupBuilder app)
        {
            app.MapDelete("capacitacion/sesiones/{id:guid}/participantes/{employeeNumber}", async (Guid id, string employeeNumber, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.RemoveAttendeeAsync(id, employeeNumber, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Quita un participante de la sesión")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
