using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class ApproveDocumentRevisionEndpoint
    {
        private const string ENDPOINT_NAME = "Aprobar Revision de Documento";

        public static RouteGroupBuilder MapApproveDocumentRevision(this RouteGroupBuilder app)
        {
            app.MapPost("documentos/{id:guid}/revisiones/pendiente/aprobar", async (Guid id, [FromBody] OptionalDateRequest request, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.ApproveAsync(id, request.Date, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Aprueba con firma electrónica la revisión pendiente y la vuelve vigente")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
