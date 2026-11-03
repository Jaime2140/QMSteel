using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class ReturnWpsToDraftEndpoint
    {
        private const string ENDPOINT_NAME = "Regresar WPS a Borrador";

        public static RouteGroupBuilder MapReturnWpsToDraft(this RouteGroupBuilder app)
        {
            app.MapPost("wps/{id:guid}/regresar-borrador", async (Guid id, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.ReturnToDraftAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Regresa a borrador un WPS en revisión")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
