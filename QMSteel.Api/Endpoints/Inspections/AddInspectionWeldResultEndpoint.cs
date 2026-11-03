using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class AddInspectionWeldResultEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Resultado de Soldadura";

        public static RouteGroupBuilder MapAddInspectionWeldResult(this RouteGroupBuilder app)
        {
            app.MapPost("inspecciones/{id:guid}/soldaduras", async (Guid id, [FromBody] AddWeldResultRequest request, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.AddWeldResultAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Captura el resultado por junta; un soldador no calificado se bloquea")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
