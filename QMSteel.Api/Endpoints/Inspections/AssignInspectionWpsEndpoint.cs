using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class AssignInspectionWpsEndpoint
    {
        private const string ENDPOINT_NAME = "Asignar WPS a Inspeccion";

        public static RouteGroupBuilder MapAssignInspectionWps(this RouteGroupBuilder app)
        {
            app.MapPut("inspecciones/{id:guid}/wps/{wpsId:guid}", async (Guid id, Guid wpsId, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.AssignWpsAsync(id, wpsId, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Asigna el WPS aprobado; uno en revisión u obsoleto se bloquea")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
