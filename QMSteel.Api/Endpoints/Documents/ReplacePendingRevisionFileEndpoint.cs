using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class ReplacePendingRevisionFileEndpoint
    {
        private const string ENDPOINT_NAME = "Reemplazar Archivo de Revision";

        public static RouteGroupBuilder MapReplacePendingRevisionFile(this RouteGroupBuilder app)
        {
            app.MapPut("documentos/{id:guid}/revisiones/pendiente/archivo", async (Guid id, [FromBody] FileRequest request, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.ReplacePendingFileAsync(id, request.FileId, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Sustituye el archivo de la revisión que aún no se aprueba")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
