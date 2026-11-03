using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Compliance;

namespace QMSteel.Api.Endpoints.QualityObjectives
{
    public static class GetQualityObjectiveEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Objetivo de Calidad";

        public static RouteGroupBuilder MapGetQualityObjective(this RouteGroupBuilder app)
        {
            app.MapGet("objetivos/{id:guid}", async (Guid id, QualityObjectiveService qualityObjectiveService, CancellationToken ct) =>
            {
                var result = await qualityObjectiveService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<QualityObjectiveDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Detalle del objetivo con sus mediciones mensuales")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
