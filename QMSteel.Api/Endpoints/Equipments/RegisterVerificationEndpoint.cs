using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class RegisterVerificationEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Verificacion";

        public static RouteGroupBuilder MapRegisterVerification(this RouteGroupBuilder app)
        {
            app.MapPost("equipos/{id:guid}/verificaciones", async (Guid id, [FromBody] RegisterVerificationRequest request, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var result = await equipmentService.RegisterVerificationAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<EquipmentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra la verificación diaria o periódica con firma electrónica")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
