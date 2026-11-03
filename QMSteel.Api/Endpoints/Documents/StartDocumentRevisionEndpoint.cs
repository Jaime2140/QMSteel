using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class StartDocumentRevisionEndpoint
    {
        private const string ENDPOINT_NAME = "Iniciar Revision de Documento";

        public static RouteGroupBuilder MapStartDocumentRevision(this RouteGroupBuilder app)
        {
            app.MapPost("documentos/{id:guid}/revisiones", async (Guid id, [FromBody] StartRevisionRequest request, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.StartRevisionAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status403Forbidden)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Crea una nueva revisión con el archivo y la firma de quien elabora")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
