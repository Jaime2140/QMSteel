using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class UpdateDocumentEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar Documento";

        public static RouteGroupBuilder MapUpdateDocument(this RouteGroupBuilder app)
        {
            app.MapPut("documentos/{id:guid}", async (Guid id, [FromBody] UpdateDocumentRequest request, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.UpdateAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cambia el título y las cláusulas de la norma que cubre el documento")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
