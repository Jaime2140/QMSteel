using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class RejectDocumentRevisionEndpoint
    {
        private const string ENDPOINT_NAME = "Rechazar Revision de Documento";

        public static RouteGroupBuilder MapRejectDocumentRevision(this RouteGroupBuilder app)
        {
            app.MapPost("documentos/{id:guid}/revisiones/pendiente/rechazar", async (Guid id, [FromBody] ReasonRequest request, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.RejectAsync(id, request.Reason, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Regresa la revisión pendiente con el motivo del rechazo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
