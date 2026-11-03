using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class RetireEquipmentEndpoint
    {
        private const string ENDPOINT_NAME = "Dar de Baja Equipo";

        public static RouteGroupBuilder MapRetireEquipment(this RouteGroupBuilder app)
        {
            app.MapPost("equipos/{id:guid}/baja", async (Guid id, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var result = await equipmentService.RetireAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<EquipmentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Da de baja el equipo del inventario")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
