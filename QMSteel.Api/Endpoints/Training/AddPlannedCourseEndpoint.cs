using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Training;

namespace QMSteel.Api.Endpoints.Training
{
    public static class AddPlannedCourseEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Curso al Plan";

        public static RouteGroupBuilder MapAddPlannedCourse(this RouteGroupBuilder app)
        {
            app.MapPost("capacitacion/planes/{id:guid}/cursos", async (Guid id, [FromBody] AddCourseRequest request, TrainingService trainingService, CancellationToken ct) =>
            {
                var result = await trainingService.AddCourseAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<TrainingPlanDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Agrega un curso programado al plan anual")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
