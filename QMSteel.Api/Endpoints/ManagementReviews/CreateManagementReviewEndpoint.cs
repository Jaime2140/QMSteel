using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.ManagementReviews
{
    public static class CreateManagementReviewEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Revision por la Direccion";

        public static RouteGroupBuilder MapCreateManagementReview(this RouteGroupBuilder app)
        {
            app.MapPost("revisiones-direccion", async ([FromBody] CreateManagementReviewRequest request, ManagementReviewService managementReviewService, CancellationToken ct) =>
            {
                var result = await managementReviewService.CreateAsync(request, ct);
                return result.ToCreatedResult(managementReview => $"/api/revisiones-direccion/{managementReview.Id}");
            })
            .Produces<ManagementReviewDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra el acta de revisión por la dirección")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
