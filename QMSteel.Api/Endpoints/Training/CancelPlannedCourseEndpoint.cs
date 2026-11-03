using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class CancelPlannedCourseEndpoint
    {
        private const string ENDPOINT_NAME = "Cancelar Curso del Plan";

        public static RouteGroupBuilder MapCancelPlannedCourse(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/planes/{id:guid}/cursos/{courseId:guid}/cancelar", async (Guid id, Guid courseId, [FromBody] ReasonRequest request, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.CancelCourseAsync(id, courseId, request.Reason, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingPlanDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cancela un curso programado indicando el motivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
