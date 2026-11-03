using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class SubmitDocumentRevisionEndpoint
    {
        private const string ENDPOINT_NAME = "Enviar Revision a Aprobacion";

        public static RouteGroupBuilder MapSubmitDocumentRevision(this RouteGroupBuilder app)
        {
            app.MapPost("documentos/{id:guid}/revisiones/pendiente/enviar", async (Guid id, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.SubmitForApprovalAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Firma la revisión como revisada y la envía a aprobación")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
