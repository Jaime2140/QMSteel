using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Users;

namespace QMSteel.Api.Endpoints.Users
{
    public static class ListUsersEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Usuarios";

        public static RouteGroupBuilder MapListUsers(this RouteGroupBuilder app)
        {
            app.MapGet("usuarios", async ([FromQuery(Name = "soloActivos")] bool? onlyActive, UserService userService, CancellationToken ct) =>
            {
                var response = await userService.ListAsync(onlyActive, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<UserDto>>(StatusCodes.Status200OK)
            .WithDescription("Usuarios de la planta")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
