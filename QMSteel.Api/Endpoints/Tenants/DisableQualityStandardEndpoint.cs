using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;
using QMSteel.Domain.Entities.Users;
using QMSteel.Domain.Standards;

namespace QMSteel.Api.Endpoints.Tenants
{
    public static class DisableQualityStandardEndpoint
    {
        private const string ENDPOINT_NAME = "Deshabilitar Norma";

        public static RouteGroupBuilder MapDisableQualityStandard(this RouteGroupBuilder app)
        {
            app.MapDelete("planta/normas/{standard}", async (QualityStandard standard, TenantService tenantService, CancellationToken ct) =>
            {
                var result = await tenantService.DisableStandardAsync(standard, ct);
                return result.ToHttpResult();
            })
            .RequireRoles(UserRole.Administrator)
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Deshabilita una norma de la planta")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
