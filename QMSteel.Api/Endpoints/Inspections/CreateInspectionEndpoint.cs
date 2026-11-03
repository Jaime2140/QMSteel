using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class CreateInspectionEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Inspeccion";

        public static RouteGroupBuilder MapCreateInspection(this RouteGroupBuilder app)
        {
            app.MapPost("inspecciones", async ([FromBody] CreateInspectionRequest request, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.CreateAsync(request, ct);
                return result.ToCreatedResult(inspection => $"/api/inspecciones/{inspection.Id}");
            })
            .Produces<InspectionDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Abre un reporte de inspección NDT o de recubrimiento con folio consecutivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
