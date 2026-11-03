using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class GetWelderHistoryEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Historial de Soldaduras";

        public static RouteGroupBuilder MapGetWelderHistory(this RouteGroupBuilder app)
        {
            app.MapGet("soldadores/{id:guid}/historial", async (Guid id, [FromQuery(Name = "cantidad")] int? take, WelderService welderService, CancellationToken ct) =>
            {
                var result = await welderService.GetHistoryAsync(id, take ?? 20, ct);
                return result.ToHttpResult();
            })
            .Produces<IReadOnlyList<WeldHistoryDto>>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Soldaduras recientes del soldador tomadas de las inspecciones")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
