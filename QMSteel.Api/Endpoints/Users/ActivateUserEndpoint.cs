using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;
using QMSteel.Domain.Entities.Users;

namespace QMSteel.Api.Endpoints.Users
{
    public static class ActivateUserEndpoint
    {
        private const string ENDPOINT_NAME = "Activar Usuario";

        public static RouteGroupBuilder MapActivateUser(this RouteGroupBuilder app)
        {
            app.MapPost("usuarios/{id:guid}/activar", async (Guid id, UserService userService, CancellationToken ct) =>
            {
                var result = await userService.ActivateAsync(id, ct);
                return result.ToHttpResult();
            })
            .RequireRoles(UserRole.Administrator)
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Reactiva a un usuario inactivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
