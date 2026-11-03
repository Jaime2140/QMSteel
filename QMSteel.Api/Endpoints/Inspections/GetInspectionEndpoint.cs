using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class GetInspectionEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Inspeccion";

        public static RouteGroupBuilder MapGetInspection(this RouteGroupBuilder app)
        {
            app.MapGet("inspecciones/{id:guid}", async (Guid id, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Reporte de inspección con resultados y cálculos de aceptación")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
