using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Inspections;

namespace QMSteel.Api.Endpoints.Inspections
{
    public static class SignInspectionEndpoint
    {
        private const string ENDPOINT_NAME = "Firmar Inspeccion";

        public static RouteGroupBuilder MapSignInspection(this RouteGroupBuilder app)
        {
            app.MapPost("inspecciones/{id:guid}/firmar", async (Guid id, InspectionService inspectionService, CancellationToken ct) =>
            {
                var result = await inspectionService.SignAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectionDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Firma el reporte; aplica el candado de certificación y examen de la vista del inspector")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
