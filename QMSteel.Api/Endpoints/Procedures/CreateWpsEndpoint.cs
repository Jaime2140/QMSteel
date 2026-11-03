using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class CreateWpsEndpoint
    {
        private const string ENDPOINT_NAME = "Crear WPS";

        public static RouteGroupBuilder MapCreateWps(this RouteGroupBuilder app)
        {
            app.MapPost("wps", async ([FromBody] CreateWpsRequest request, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.CreateWpsAsync(request, ct);
                return result.ToCreatedResult(wps => $"/api/wps/{wps.Id}");
            })
            .Produces<WpsDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra un WPS con sus variables esenciales")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
