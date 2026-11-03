using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class ApproveWpsEndpoint
    {
        private const string ENDPOINT_NAME = "Aprobar WPS";

        public static RouteGroupBuilder MapApproveWps(this RouteGroupBuilder app)
        {
            app.MapPost("wps/{id:guid}/aprobar", async (Guid id, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.ApproveWpsAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Valida contra los PQR (AWS D1.1) y aprueba el WPS con firma electrónica")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
