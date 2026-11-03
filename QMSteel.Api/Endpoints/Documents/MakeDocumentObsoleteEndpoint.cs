using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class MakeDocumentObsoleteEndpoint
    {
        private const string ENDPOINT_NAME = "Marcar Documento Obsoleto";

        public static RouteGroupBuilder MapMakeDocumentObsolete(this RouteGroupBuilder app)
        {
            app.MapPost("documentos/{id:guid}/obsoleto", async (Guid id, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.MakeObsoleteAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Declara obsoleto el documento y revoca sus copias controladas")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
