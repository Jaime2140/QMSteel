using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Personnel;

namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class RegisterVisionExamEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Examen de la Vista";

        public static RouteGroupBuilder MapRegisterVisionExam(this RouteGroupBuilder app)
        {
            app.MapPost("inspectores/{id:guid}/examenes-vista", async (Guid id, [FromBody] RegisterVisionExamRequest request, InspectorService inspectorService, CancellationToken ct) =>
            {
                var result = await inspectorService.RegisterVisionExamAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectorDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra el examen anual de la vista (Jaeger J1 e Ishihara)")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
