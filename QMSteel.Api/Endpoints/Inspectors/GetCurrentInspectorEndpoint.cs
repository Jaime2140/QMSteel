using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Personnel;

namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class GetCurrentInspectorEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Inspector Actual";

        public static RouteGroupBuilder MapGetCurrentInspector(this RouteGroupBuilder app)
        {
            app.MapGet("inspectores/actual", async (InspectorService inspectorService, CancellationToken ct) =>
            {
                var result = await inspectorService.GetCurrentAsync(ct);
                return result.ToHttpResult();
            })
            .Produces<InspectorDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .WithDescription("Expediente del inspector ligado al usuario en sesión y sus permisos de firma")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
