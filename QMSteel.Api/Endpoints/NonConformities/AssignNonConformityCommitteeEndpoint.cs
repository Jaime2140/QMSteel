using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class AssignNonConformityCommitteeEndpoint
    {
        private const string ENDPOINT_NAME = "Asignar Comision de NC";

        public static RouteGroupBuilder MapAssignNonConformityCommittee(this RouteGroupBuilder app)
        {
            app.MapPut("no-conformidades/{id:guid}/comision", async (Guid id, [FromBody] CommitteeRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.AssignCommitteeAsync(id, request.MemberUserIds, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Forma la comisión de atención de la acción correctiva")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
