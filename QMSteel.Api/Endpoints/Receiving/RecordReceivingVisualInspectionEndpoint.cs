using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Receiving
{
    public static class RecordReceivingVisualInspectionEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Inspeccion Visual";

        public static RouteGroupBuilder MapRecordReceivingVisualInspection(this RouteGroupBuilder app)
        {
            app.MapPost("recepciones/{id:guid}/partidas/{itemId:guid}/inspeccion-visual", async (Guid id, Guid itemId, [FromBody] VisualInspectionRequest request, ReceivingService receivingService, CancellationToken ct) =>
            {
                var result = await receivingService.RecordVisualInspectionAsync(id, itemId, request, ct);
                return result.ToHttpResult();
            })
            .Produces<ReceivingDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra la inspección visual de la partida y sus defectos")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
