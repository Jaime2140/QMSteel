using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class DeactivateWelderEndpoint
    {
        private const string ENDPOINT_NAME = "Desactivar Soldador";

        public static RouteGroupBuilder MapDeactivateWelder(this RouteGroupBuilder app)
        {
            app.MapPost("soldadores/{id:guid}/desactivar", async (Guid id, WelderService welderService, CancellationToken ct) =>
            {
                var result = await welderService.DeactivateAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<WelderDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Desactiva al soldador; deja de aparecer en las inspecciones")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
