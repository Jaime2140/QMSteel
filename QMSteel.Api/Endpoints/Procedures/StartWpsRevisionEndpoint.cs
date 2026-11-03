using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class StartWpsRevisionEndpoint
    {
        private const string ENDPOINT_NAME = "Iniciar Nueva Revision de WPS";

        public static RouteGroupBuilder MapStartWpsRevision(this RouteGroupBuilder app)
        {
            app.MapPost("wps/{id:guid}/nueva-revision", async (Guid id, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.StartNewRevisionAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Abre una nueva revisión del WPS aprobado")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
