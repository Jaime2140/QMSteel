using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class UnlinkPqrFromWpsEndpoint
    {
        private const string ENDPOINT_NAME = "Desvincular PQR de WPS";

        public static RouteGroupBuilder MapUnlinkPqrFromWps(this RouteGroupBuilder app)
        {
            app.MapDelete("wps/{id:guid}/pqr/{pqrId:guid}", async (Guid id, Guid pqrId, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.UnlinkPqrAsync(id, pqrId, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Quita un PQR de respaldo del WPS")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
