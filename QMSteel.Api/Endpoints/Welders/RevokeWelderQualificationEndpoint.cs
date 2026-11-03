using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class RevokeWelderQualificationEndpoint
    {
        private const string ENDPOINT_NAME = "Revocar Calificacion de Soldador";

        public static RouteGroupBuilder MapRevokeWelderQualification(this RouteGroupBuilder app)
        {
            app.MapPost("soldadores/{id:guid}/calificaciones/{qualificationId:guid}/revocar", async (Guid id, Guid qualificationId, [FromBody] ReasonRequest request, WelderService welderService, CancellationToken ct) =>
            {
                var result = await welderService.RevokeQualificationAsync(id, qualificationId, request.Reason, ct);
                return result.ToHttpResult();
            })
            .Produces<WelderDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Revoca una calificación indicando el motivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
