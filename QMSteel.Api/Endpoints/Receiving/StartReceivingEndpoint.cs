using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Receiving
{
    public static class StartReceivingEndpoint
    {
        private const string ENDPOINT_NAME = "Iniciar Recepcion";

        public static RouteGroupBuilder MapStartReceiving(this RouteGroupBuilder app)
        {
            app.MapPost("recepciones", async ([FromBody] StartReceivingRequest request, ReceivingService receivingService, CancellationToken ct) =>
            {
                var result = await receivingService.StartAsync(request, ct);
                return result.ToCreatedResult(receiving => $"/api/recepciones/{receiving.Id}");
            })
            .Produces<ReceivingDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Abre la inspección de recibo de una remisión de un proveedor aprobado")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
