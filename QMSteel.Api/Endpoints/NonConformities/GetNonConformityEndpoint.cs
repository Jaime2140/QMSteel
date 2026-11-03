using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class GetNonConformityEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar NC";

        public static RouteGroupBuilder MapGetNonConformity(this RouteGroupBuilder app)
        {
            app.MapGet("no-conformidades/{id:guid}", async (Guid id, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Detalle de la NC con análisis de causa raíz y acciones")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
