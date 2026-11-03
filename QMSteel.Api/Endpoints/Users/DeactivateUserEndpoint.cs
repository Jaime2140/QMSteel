using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;
using QMSteel.Domain.Entities.Users;

namespace QMSteel.Api.Endpoints.Users
{
    public static class DeactivateUserEndpoint
    {
        private const string ENDPOINT_NAME = "Desactivar Usuario";

        public static RouteGroupBuilder MapDeactivateUser(this RouteGroupBuilder app)
        {
            app.MapPost("usuarios/{id:guid}/desactivar", async (Guid id, UserService userService, CancellationToken ct) =>
            {
                var result = await userService.DeactivateAsync(id, ct);
                return result.ToHttpResult();
            })
            .RequireRoles(UserRole.Administrator)
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Desactiva al usuario; ya no puede entrar al sistema")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
