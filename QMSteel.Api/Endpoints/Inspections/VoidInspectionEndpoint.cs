using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class VoidInspectionEndpoint
    {
        private const string ENDPOINT_NAME = "Anular Inspeccion";

        public static RouteGroupBuilder MapVoidInspection(this RouteGroupBuilder app)
        {
            app.MapPost("inspecciones/{id:guid}/anular", async (Guid id, [FromBody] ReasonRequest request, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.VoidAsync(id, request.Reason, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Anula el reporte indicando el motivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
