using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Procedures;

namespace QMSteel.Api.Endpoints.Procedures
{
    public static class AttachPqrFileEndpoint
    {
        private const string ENDPOINT_NAME = "Adjuntar Archivo de PQR";

        public static RouteGroupBuilder MapAttachPqrFile(this RouteGroupBuilder app)
        {
            app.MapPut("pqr/{id:guid}/archivo", async (Guid id, [FromBody] FileRequest request, ProcedureService procedureService, CancellationToken ct) =>
            {
                var result = await procedureService.AttachPqrFileAsync(id, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<PqrDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Adjunta el PDF firmado del PQR")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
