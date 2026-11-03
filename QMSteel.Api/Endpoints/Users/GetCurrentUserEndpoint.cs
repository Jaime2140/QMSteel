using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Users;

namespace QMSteel.Api.Endpoints.Users
{
    public static class GetCurrentUserEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Usuario Actual";

        public static RouteGroupBuilder MapGetCurrentUser(this RouteGroupBuilder app)
        {
            app.MapGet("usuarios/actual", async (UserService userService, CancellationToken ct) =>
            {
                var result = await userService.GetCurrentAsync(ct);
                return result.ToHttpResult();
            })
            .Produces<UserDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Perfil y rol del usuario en sesión")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
