using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class ListWeldersEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Soldadores";

        public static RouteGroupBuilder MapListWelders(this RouteGroupBuilder app)
        {
            app.MapGet("soldadores", async ([FromQuery(Name = "soloActivos")] bool? onlyActive, WelderService welderService, CancellationToken ct) =>
            {
                var response = await welderService.ListAsync(onlyActive, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<WelderSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Catálogo de soldadores y punteadores con estatus y alerta de continuidad")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
