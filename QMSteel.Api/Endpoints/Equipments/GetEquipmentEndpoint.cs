using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class GetEquipmentEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Equipo";

        public static RouteGroupBuilder MapGetEquipment(this RouteGroupBuilder app)
        {
            app.MapGet("equipos/{id:guid}", async (Guid id, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var result = await equipmentService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<EquipmentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Expediente del equipo con calibraciones, verificaciones y frecuencia de uso")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
