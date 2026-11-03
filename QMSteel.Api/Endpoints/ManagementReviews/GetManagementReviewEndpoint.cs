using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.ManagementReviews
{
    public static class GetManagementReviewEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Revision por la Direccion";

        public static RouteGroupBuilder MapGetManagementReview(this RouteGroupBuilder app)
        {
            app.MapGet("revisiones-direccion/{id:guid}", async (Guid id, ManagementReviewService managementReviewService, CancellationToken ct) =>
            {
                var result = await managementReviewService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<ManagementReviewDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Detalle del acta con sus decisiones")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
