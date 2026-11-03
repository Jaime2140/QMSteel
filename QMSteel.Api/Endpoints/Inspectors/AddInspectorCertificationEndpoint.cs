using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Personnel;

namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class AddInspectorCertificationEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Certificacion de Inspector";

        public static RouteGroupBuilder MapAddInspectorCertification(this RouteGroupBuilder app)
        {
            app.MapPost("inspectores/{id:guid}/certificaciones", async (Guid id, [FromBody] AddCertificationRequest request, InspectorService inspectorService, CancellationToken ct) =>
            {
                var result = await inspectorService.AddCertificationAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectorDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra una certificación (CWI, SNT-TC-1A) por método y nivel")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
