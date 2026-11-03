using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;
using QMSteel.Domain.Entities.Users;

namespace QMSteel.Api.Endpoints.Users
{
    public static class CreateUserEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Usuario";

        public static RouteGroupBuilder MapCreateUser(this RouteGroupBuilder app)
        {
            app.MapPost("usuarios", async ([FromBody] CreateUserRequest request, UserService userService, CancellationToken ct) =>
            {
                var result = await userService.CreateAsync(request, ct);
                return result.ToCreatedResult(user => $"/api/usuarios/{user.Id}");
            })
            .RequireRoles(UserRole.Administrator)
            .Produces<UserDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Da de alta a un usuario de la planta ligado a su identidad del proveedor")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
