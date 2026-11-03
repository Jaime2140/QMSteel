using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Welders;
using QMSteel.Domain.Welding;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class ListEligibleWeldersEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Soldadores Elegibles";

        public static RouteGroupBuilder MapListEligibleWelders(this RouteGroupBuilder app)
        {
            app.MapGet("soldadores/elegibles", async ([FromQuery(Name = "proceso")] WeldingProcess? process, [FromQuery(Name = "posicion")] string? position, [FromQuery(Name = "punteo")] bool? isTackWeld, WelderService welderService, CancellationToken ct) =>
            {
                var response = await welderService.ListEligibleAsync(process, position, isTackWeld ?? false, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<WelderSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Soldadores que hoy pueden soldar el proceso y la posición indicados")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
