using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;
using QMSteel.Domain.Entities.Users;

namespace QMSteel.Api.Endpoints.Users
{
    public static class UpdateUserEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar Usuario";

        public static RouteGroupBuilder MapUpdateUser(this RouteGroupBuilder app)
        {
            app.MapPut("usuarios/{id:guid}", async (Guid id, [FromBody] UpdateUserRequest request, UserService userService, CancellationToken ct) =>
            {
                var result = await userService.UpdateAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .RequireRoles(UserRole.Administrator)
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cambia el nombre, puesto y área del usuario")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
