using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class GetDocumentEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Documento";

        public static RouteGroupBuilder MapGetDocument(this RouteGroupBuilder app)
        {
            app.MapGet("documentos/{id:guid}", async (Guid id, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Expediente del documento con historial de revisiones y copias controladas")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
