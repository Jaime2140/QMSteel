using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class IssueControlledCopyEndpoint
    {
        private const string ENDPOINT_NAME = "Emitir Copia Controlada";

        public static RouteGroupBuilder MapIssueControlledCopy(this RouteGroupBuilder app)
        {
            app.MapPost("documentos/{id:guid}/copias", async (Guid id, [FromBody] IssueCopyRequest request, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.IssueCopyAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Emite una copia controlada de la revisión vigente para un destinatario")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
