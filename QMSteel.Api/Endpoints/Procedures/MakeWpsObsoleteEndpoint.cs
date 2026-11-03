using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class MakeWpsObsoleteEndpoint
    {
        private const string ENDPOINT_NAME = "Marcar WPS Obsoleto";

        public static RouteGroupBuilder MapMakeWpsObsolete(this RouteGroupBuilder app)
        {
            app.MapPost("wps/{id:guid}/obsoleto", async (Guid id, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.MakeWpsObsoleteAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Declara obsoleto el WPS; deja de poder seleccionarse e imprimirse")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
