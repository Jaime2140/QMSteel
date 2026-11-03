using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class RecordTrainingAttendanceEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Asistencia";

        public static RouteGroupBuilder MapRecordTrainingAttendance(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/sesiones/{id:guid}/asistencia", async (Guid id, [FromBody] RecordAttendanceRequest request, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.RecordAttendanceAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingSessionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra la asistencia y la calificación de un participante")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
