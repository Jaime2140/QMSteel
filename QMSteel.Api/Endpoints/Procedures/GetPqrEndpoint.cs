using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class GetPqrEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar PQR";

        public static RouteGroupBuilder MapGetPqr(this RouteGroupBuilder app)
        {
            app.MapGet("pqr/{id:guid}", async (Guid id, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.GetPqrAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<PqrDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Detalle del PQR con sus variables esenciales")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
