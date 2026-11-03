using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class UpdatePqrVariablesEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar Variables de PQR";

        public static RouteGroupBuilder MapUpdatePqrVariables(this RouteGroupBuilder app)
        {
            app.MapPut("pqr/{id:guid}/variables", async (Guid id, [FromBody] EssentialVariablesDto request, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.UpdatePqrVariablesAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<PqrDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Corrige las variables esenciales de un PQR en borrador")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
