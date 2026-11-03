using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.ManagementReviews
{
    public static class ApproveManagementReviewEndpoint
    {
        private const string ENDPOINT_NAME = "Aprobar Revision por la Direccion";

        public static RouteGroupBuilder MapApproveManagementReview(this RouteGroupBuilder app)
        {
            app.MapPost("revisiones-direccion/{id:guid}/aprobar", async (Guid id, ManagementReviewService managementReviewService, CancellationToken ct) =>
            {
                var result = await managementReviewService.ApproveAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<ManagementReviewDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Aprueba el acta con firma electrónica")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
