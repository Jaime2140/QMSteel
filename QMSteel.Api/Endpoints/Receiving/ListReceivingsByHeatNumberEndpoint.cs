using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Receiving
{
    public static class ListReceivingsByHeatNumberEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Recepciones por Colada";

        public static RouteGroupBuilder MapListReceivingsByHeatNumber(this RouteGroupBuilder app)
        {
            app.MapGet("recepciones/colada/{heatNumber}", async (string heatNumber, ReceivingService receivingService, CancellationToken ct) =>
            {
                var response = await receivingService.ListByHeatNumberAsync(heatNumber, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<ReceivingDto>>(StatusCodes.Status200OK)
            .WithDescription("Trazabilidad del material por número de colada")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
