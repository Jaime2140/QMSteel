using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class CreatePqrEndpoint
    {
        private const string ENDPOINT_NAME = "Crear PQR";

        public static RouteGroupBuilder MapCreatePqr(this RouteGroupBuilder app)
        {
            app.MapPost("pqr", async ([FromBody] CreatePqrRequest request, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.CreatePqrAsync(request, ct);
                return result.ToCreatedResult(pqr => $"/api/pqr/{pqr.Id}");
            })
            .Produces<PqrDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra un PQR con sus variables esenciales")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
