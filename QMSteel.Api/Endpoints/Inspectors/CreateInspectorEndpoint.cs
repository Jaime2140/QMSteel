using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Personnel;

namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class CreateInspectorEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Inspector";

        public static RouteGroupBuilder MapCreateInspector(this RouteGroupBuilder app)
        {
            app.MapPost("inspectores", async ([FromBody] CreateInspectorRequest request, InspectorService inspectorService, CancellationToken ct) =>
            {
                var result = await inspectorService.CreateAsync(request, ct);
                return result.ToCreatedResult(inspector => $"/api/inspectores/{inspector.Id}");
            })
            .Produces<InspectorDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Da de alta un inspector y opcionalmente lo liga a un usuario")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
