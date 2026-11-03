using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class RegisterCalibrationEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Calibracion";

        public static RouteGroupBuilder MapRegisterCalibration(this RouteGroupBuilder app)
        {
            app.MapPost("equipos/{id:guid}/calibraciones", async (Guid id, [FromBody] RegisterCalibrationRequest request, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var result = await equipmentService.RegisterCalibrationAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<EquipmentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra la calibración externa del laboratorio acreditado")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
