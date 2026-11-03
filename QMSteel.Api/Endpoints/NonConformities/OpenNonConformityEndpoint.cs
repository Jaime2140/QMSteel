using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class OpenNonConformityEndpoint
    {
        private const string ENDPOINT_NAME = "Abrir NC";

        public static RouteGroupBuilder MapOpenNonConformity(this RouteGroupBuilder app)
        {
            app.MapPost("no-conformidades", async ([FromBody] OpenNonConformityRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.OpenAsync(request, ct);
                return result.ToCreatedResult(nonConformity => $"/api/no-conformidades/{nonConformity.Id}");
            })
            .Produces<NonConformityDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra una desviación (paso 1: detalles iniciales y evidencias)")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
