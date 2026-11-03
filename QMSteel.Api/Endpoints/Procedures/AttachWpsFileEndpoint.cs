using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class AttachWpsFileEndpoint
    {
        private const string ENDPOINT_NAME = "Adjuntar Archivo de WPS";

        public static RouteGroupBuilder MapAttachWpsFile(this RouteGroupBuilder app)
        {
            app.MapPut("wps/{id:guid}/archivo", async (Guid id, [FromBody] FileRequest request, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.AttachWpsFileAsync(id, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<WpsDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Adjunta el PDF firmado del WPS")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
