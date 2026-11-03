using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class RemoveInspectionItemEndpoint
    {
        private const string ENDPOINT_NAME = "Quitar Partida de Inspeccion";

        public static RouteGroupBuilder MapRemoveInspectionItem(this RouteGroupBuilder app)
        {
            app.MapDelete("inspecciones/{id:guid}/partidas/{itemId:guid}", async (Guid id, Guid itemId, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.RemoveItemAsync(id, itemId, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Quita un resultado capturado del reporte")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
