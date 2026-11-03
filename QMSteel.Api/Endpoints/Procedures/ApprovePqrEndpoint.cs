using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class ApprovePqrEndpoint
    {
        private const string ENDPOINT_NAME = "Aprobar PQR";

        public static RouteGroupBuilder MapApprovePqr(this RouteGroupBuilder app)
        {
            app.MapPost("pqr/{id:guid}/aprobar", async (Guid id, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.ApprovePqrAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<PqrDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Aprueba el PQR con firma electrónica")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
