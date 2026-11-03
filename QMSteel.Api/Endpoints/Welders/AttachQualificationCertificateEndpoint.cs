using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class AttachQualificationCertificateEndpoint
    {
        private const string ENDPOINT_NAME = "Adjuntar Certificado WPQR";

        public static RouteGroupBuilder MapAttachQualificationCertificate(this RouteGroupBuilder app)
        {
            app.MapPut("soldadores/{id:guid}/calificaciones/{qualificationId:guid}/certificado", async (Guid id, Guid qualificationId, [FromBody] FileRequest request, WelderService welderService, CancellationToken ct) =>
            {
                var result = await welderService.AttachCertificateAsync(id, qualificationId, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<WelderDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Adjunta el PDF del certificado de calificación")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
