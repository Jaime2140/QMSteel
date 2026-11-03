using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.ManagementReviews
{
    public static class SetManagementReviewAttendeesEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar Asistentes de Revision";

        public static RouteGroupBuilder MapSetManagementReviewAttendees(this RouteGroupBuilder app)
        {
            app.MapPut("revisiones-direccion/{id:guid}/asistentes", async (Guid id, [FromBody] AttendeesRequest request, ManagementReviewService managementReviewService, CancellationToken ct) =>
            {
                var result = await managementReviewService.SetAttendeesAsync(id, request.Attendees, ct);
                return result.ToHttpResult();
            })
            .Produces<ManagementReviewDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Reemplaza la lista de asistentes del acta")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
