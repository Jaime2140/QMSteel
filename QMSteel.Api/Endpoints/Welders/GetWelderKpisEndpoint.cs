using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class GetWelderKpisEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Indicadores de Soldadores";

        public static RouteGroupBuilder MapGetWelderKpis(this RouteGroupBuilder app)
        {
            app.MapGet("soldadores/indicadores", async (WelderService welderService, CancellationToken ct) =>
            {
                var response = await welderService.GetKpisAsync(ct);
                return Results.Ok(response);
            })
            .Produces<WelderKpisDto>(StatusCodes.Status200OK)
            .WithDescription("Totales de soldadores activos, vigentes, por vencer y vencidos o inactivos")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
