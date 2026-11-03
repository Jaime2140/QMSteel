using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class ActivateWelderEndpoint
    {
        private const string ENDPOINT_NAME = "Activar Soldador";

        public static RouteGroupBuilder MapActivateWelder(this RouteGroupBuilder app)
        {
            app.MapPost("soldadores/{id:guid}/activar", async (Guid id, WelderService welderService, CancellationToken ct) =>
            {
                var result = await welderService.ActivateAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<WelderDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Reactiva a un soldador inactivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
