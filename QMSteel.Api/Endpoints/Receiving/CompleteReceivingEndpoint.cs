using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Receiving
{
    public static class CompleteReceivingEndpoint
    {
        private const string ENDPOINT_NAME = "Cerrar Recepcion";

        public static RouteGroupBuilder MapCompleteReceiving(this RouteGroupBuilder app)
        {
            app.MapPost("recepciones/{id:guid}/cerrar", async (Guid id, ReceivingService receivingService, CancellationToken ct) =>
            {
                var result = await receivingService.CompleteAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<ReceivingDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cierra la recepción con firma; si hay desviaciones se abre la NCR automáticamente")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
