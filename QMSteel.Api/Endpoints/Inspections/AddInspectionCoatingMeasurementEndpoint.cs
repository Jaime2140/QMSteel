using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class AddInspectionCoatingMeasurementEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Medicion de Recubrimiento";

        public static RouteGroupBuilder MapAddInspectionCoatingMeasurement(this RouteGroupBuilder app)
        {
            app.MapPost("inspecciones/{id:guid}/mediciones-recubrimiento", async (Guid id, [FromBody] AddCoatingMeasurementRequest request, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.AddCoatingMeasurementAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Captura las lecturas de espesor (WFT/DFT) de una zona")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
