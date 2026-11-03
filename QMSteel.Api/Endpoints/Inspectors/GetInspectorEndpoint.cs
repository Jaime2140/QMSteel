using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Personnel;

namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class GetInspectorEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Inspector";

        public static RouteGroupBuilder MapGetInspector(this RouteGroupBuilder app)
        {
            app.MapGet("inspectores/{id:guid}", async (Guid id, InspectorService inspectorService, CancellationToken ct) =>
            {
                var result = await inspectorService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectorDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Expediente digital del inspector con certificaciones y exámenes de la vista")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
