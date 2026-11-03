using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class SubmitWpsForReviewEndpoint
    {
        private const string ENDPOINT_NAME = "Enviar WPS a Revision";

        public static RouteGroupBuilder MapSubmitWpsForReview(this RouteGroupBuilder app)
        {
            app.MapPost("wps/{id:guid}/enviar-revision", async (Guid id, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.SubmitForReviewAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Envía el WPS a revisión; mientras tanto no puede usarse en inspecciones")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
