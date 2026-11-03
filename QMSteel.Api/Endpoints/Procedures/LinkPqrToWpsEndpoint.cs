using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class LinkPqrToWpsEndpoint
    {
        private const string ENDPOINT_NAME = "Vincular PQR a WPS";

        public static RouteGroupBuilder MapLinkPqrToWps(this RouteGroupBuilder app)
        {
            app.MapPost("wps/{id:guid}/pqr/{pqrId:guid}", async (Guid id, Guid pqrId, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.LinkPqrAsync(id, pqrId, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Vincula un PQR de respaldo al WPS")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
