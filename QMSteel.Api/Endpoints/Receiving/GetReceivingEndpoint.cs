using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Receiving
{
    public static class GetReceivingEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Recepcion";

        public static RouteGroupBuilder MapGetReceiving(this RouteGroupBuilder app)
        {
            app.MapGet("recepciones/{id:guid}", async (Guid id, ReceivingService receivingService, CancellationToken ct) =>
            {
                var result = await receivingService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<ReceivingDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Registro de muestreo a la entrega con partidas, MTR e inspección visual")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
