using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class SetInspectionNotesEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar Notas de Inspeccion";

        public static RouteGroupBuilder MapSetInspectionNotes(this RouteGroupBuilder app)
        {
            app.MapPut("inspecciones/{id:guid}/notas", async (Guid id, [FromBody] NotesRequest request, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.SetNotesAsync(id, request.Notes, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Actualiza las observaciones del reporte")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
