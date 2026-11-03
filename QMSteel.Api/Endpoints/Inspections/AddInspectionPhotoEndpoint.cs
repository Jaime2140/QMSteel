using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class AddInspectionPhotoEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Foto a Inspeccion";

        public static RouteGroupBuilder MapAddInspectionPhoto(this RouteGroupBuilder app)
        {
            app.MapPost("inspecciones/{id:guid}/fotos", async (Guid id, [FromBody] FileRequest request, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.AddPhotoAsync(id, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Adjunta una foto al reporte")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
