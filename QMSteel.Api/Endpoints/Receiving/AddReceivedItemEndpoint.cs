using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Receiving
{
    public static class AddReceivedItemEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Partida Recibida";

        public static RouteGroupBuilder MapAddReceivedItem(this RouteGroupBuilder app)
        {
            app.MapPost("recepciones/{id:guid}/partidas", async (Guid id, [FromBody] AddReceivedItemRequest request, ReceivingService receivingService, CancellationToken ct) =>
            {
                var result = await receivingService.AddItemAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<ReceivingDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Agrega una partida de material con colada y MTR")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
