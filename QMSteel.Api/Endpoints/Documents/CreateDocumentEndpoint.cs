using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class CreateDocumentEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Documento";

        public static RouteGroupBuilder MapCreateDocument(this RouteGroupBuilder app)
        {
            app.MapPost("documentos", async ([FromBody] CreateDocumentRequest request, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.CreateAsync(request, ct);
                return result.ToCreatedResult(document => $"/api/documentos/{document.Id}");
            })
            .Produces<DocumentDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Da de alta un documento controlado en el catálogo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
