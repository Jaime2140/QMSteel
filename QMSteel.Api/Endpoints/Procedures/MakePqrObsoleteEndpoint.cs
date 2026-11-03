using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class MakePqrObsoleteEndpoint
    {
        private const string ENDPOINT_NAME = "Marcar PQR Obsoleto";

        public static RouteGroupBuilder MapMakePqrObsolete(this RouteGroupBuilder app)
        {
            app.MapPost("pqr/{id:guid}/obsoleto", async (Guid id, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.MakePqrObsoleteAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<PqrDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Declara obsoleto el PQR")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
