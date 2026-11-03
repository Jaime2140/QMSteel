using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class AddTrainingAttendeeEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Participante";

        public static RouteGroupBuilder MapAddTrainingAttendee(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/sesiones/{id:guid}/participantes", async (Guid id, [FromBody] AttendeeRequest request, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.AddAttendeeAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Agrega un participante a la sesión")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
