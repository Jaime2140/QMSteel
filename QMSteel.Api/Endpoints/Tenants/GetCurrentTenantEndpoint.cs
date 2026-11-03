using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;

namespace QMSteel.Api.Endpoints.Tenants
{
    public static class GetCurrentTenantEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Planta";

        public static RouteGroupBuilder MapGetCurrentTenant(this RouteGroupBuilder app)
        {
            app.MapGet("planta", async (TenantService tenantService, CancellationToken ct) =>
            {
                var result = await tenantService.GetCurrentAsync(ct);
                return result.ToHttpResult();
            })
            .Produces<TenantDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Datos de la planta en sesión, suscripción y normas habilitadas")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
