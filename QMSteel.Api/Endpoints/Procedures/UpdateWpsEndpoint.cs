using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class UpdateWpsEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar WPS";

        public static RouteGroupBuilder MapUpdateWps(this RouteGroupBuilder app)
        {
            app.MapPut("wps/{id:guid}", async (Guid id, [FromBody] UpdateWpsRequest request, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.UpdateWpsAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cambia la junta, el tipo de soldadura y las variables de un WPS en borrador")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
