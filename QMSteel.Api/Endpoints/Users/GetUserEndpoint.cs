using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;

namespace QMSteel.Api.Endpoints.Users
{
    public static class GetUserEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Usuario";

        public static RouteGroupBuilder MapGetUser(this RouteGroupBuilder app)
        {
            app.MapGet("usuarios/{id:guid}", async (Guid id, UserService userService, CancellationToken ct) =>
            {
                var result = await userService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Perfil de un usuario de la planta")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
