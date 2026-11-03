using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.ManagementReviews
{
    public static class ListManagementReviewsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Revisiones por la Direccion";

        public static RouteGroupBuilder MapListManagementReviews(this RouteGroupBuilder app)
        {
            app.MapGet("revisiones-direccion", async ([FromQuery(Name = "anio")] int year, ManagementReviewService managementReviewService, CancellationToken ct) =>
            {
                var response = await managementReviewService.ListByYearAsync(year, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<ManagementReviewDto>>(StatusCodes.Status200OK)
            .WithDescription("Actas de revisión por la dirección del año")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
