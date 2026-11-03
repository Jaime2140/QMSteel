using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class GetWpsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar WPS";

        public static RouteGroupBuilder MapGetWps(this RouteGroupBuilder app)
        {
            app.MapGet("wps/{id:guid}", async (Guid id, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.GetWpsAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Detalle del WPS con sus variables esenciales")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
