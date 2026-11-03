using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;
using QMSteel.Domain.Entities.Users;
using QMSteel.Domain.Standards;

namespace QMSteel.Api.Endpoints.Tenants
{
    public static class EnableQualityStandardEndpoint
    {
        private const string ENDPOINT_NAME = "Habilitar Norma";

        public static RouteGroupBuilder MapEnableQualityStandard(this RouteGroupBuilder app)
        {
            app.MapPost("planta/normas/{standard}", async (QualityStandard standard, TenantService tenantService, CancellationToken ct) =>
            {
                var result = await tenantService.EnableStandardAsync(standard, ct);
                return result.ToHttpResult();
            })
            .RequireRoles(UserRole.Administrator)
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Habilita una norma para la matriz de cumplimiento de la planta")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
