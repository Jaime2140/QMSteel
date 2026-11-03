using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class SetInspectionCleanlinessStandardEndpoint
    {
        private const string ENDPOINT_NAME = "Definir Grado de Limpieza";

        public static RouteGroupBuilder MapSetInspectionCleanlinessStandard(this RouteGroupBuilder app)
        {
            app.MapPut("inspecciones/{id:guid}/grado-limpieza", async (Guid id, [FromBody] CleanlinessRequest request, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.SetCleanlinessStandardAsync(id, request.Standard, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Indica el grado de limpieza de la preparación de superficie (ej. SSPC-SP10)")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
