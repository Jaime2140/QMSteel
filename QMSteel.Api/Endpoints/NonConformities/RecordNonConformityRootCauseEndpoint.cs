using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class RecordNonConformityRootCauseEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Causa Raiz";

        public static RouteGroupBuilder MapRecordNonConformityRootCause(this RouteGroupBuilder app)
        {
            app.MapPut("no-conformidades/{id:guid}/causa-raiz", async (Guid id, [FromBody] RootCauseRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.RecordRootCauseAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Paso 2: análisis de causa raíz con los 5 porqués")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
