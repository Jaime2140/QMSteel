using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;
using QMSteel.Domain.Entities.Users;

namespace QMSteel.Api.Endpoints.Users
{
    public static class ChangeUserRoleEndpoint
    {
        private const string ENDPOINT_NAME = "Cambiar Rol de Usuario";

        public static RouteGroupBuilder MapChangeUserRole(this RouteGroupBuilder app)
        {
            app.MapPut("usuarios/{id:guid}/rol", async (Guid id, [FromBody] RoleRequest request, UserService userService, CancellationToken ct) =>
            {
                var result = await userService.ChangeRoleAsync(id, request.Role, ct);
                return result.ToHttpResult();
            })
            .RequireRoles(UserRole.Administrator)
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Asigna el rol del usuario")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
