using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Equipments;

namespace QMSteel.Api.Endpoints.Equipments
{
    public static class AttachCalibrationCertificateEndpoint
    {
        private const string ENDPOINT_NAME = "Adjuntar Certificado de Calibracion";

        public static RouteGroupBuilder MapAttachCalibrationCertificate(this RouteGroupBuilder app)
        {
            app.MapPut("equipos/{id:guid}/calibraciones/{calibrationId:guid}/certificado", async (Guid id, Guid calibrationId, [FromBody] FileRequest request, EquipmentService equipmentService, CancellationToken ct) =>
            {
                var result = await equipmentService.AttachCalibrationCertificateAsync(id, calibrationId, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<EquipmentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Adjunta el PDF del certificado de calibración")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
