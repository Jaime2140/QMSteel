using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Receiving
{
    public static class UpdateReceivedItemMtrEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar MTR de Partida";

        public static RouteGroupBuilder MapUpdateReceivedItemMtr(this RouteGroupBuilder app)
        {
            app.MapPut("recepciones/{id:guid}/partidas/{itemId:guid}/mtr", async (Guid id, Guid itemId, [FromBody] UpdateMtrRequest request, ReceivingService receivingService, CancellationToken ct) =>
            {
                var result = await receivingService.UpdateMtrAsync(id, itemId, request, ct);
                return result.ToHttpResult();
            })
            .Produces<ReceivingDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Actualiza el estatus y el archivo del MTR de la partida")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
