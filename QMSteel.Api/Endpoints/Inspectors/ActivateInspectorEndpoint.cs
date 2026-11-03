using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Personnel;

namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class ActivateInspectorEndpoint
    {
        private const string ENDPOINT_NAME = "Activar Inspector";

        public static RouteGroupBuilder MapActivateInspector(this RouteGroupBuilder app)
        {
            app.MapPost("inspectores/{id:guid}/activar", async (Guid id, InspectorService inspectorService, CancellationToken ct) =>
            {
                var result = await inspectorService.ActivateAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectorDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Reactiva a un inspector inactivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
