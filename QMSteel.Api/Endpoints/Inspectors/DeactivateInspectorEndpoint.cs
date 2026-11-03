using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Personnel;

namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class DeactivateInspectorEndpoint
    {
        private const string ENDPOINT_NAME = "Desactivar Inspector";

        public static RouteGroupBuilder MapDeactivateInspector(this RouteGroupBuilder app)
        {
            app.MapPost("inspectores/{id:guid}/desactivar", async (Guid id, InspectorService inspectorService, CancellationToken ct) =>
            {
                var result = await inspectorService.DeactivateAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectorDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Desactiva al inspector; ya no puede firmar reportes")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
