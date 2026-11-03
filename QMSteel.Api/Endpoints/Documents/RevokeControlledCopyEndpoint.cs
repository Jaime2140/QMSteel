using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class RevokeControlledCopyEndpoint
    {
        private const string ENDPOINT_NAME = "Revocar Copia Controlada";

        public static RouteGroupBuilder MapRevokeControlledCopy(this RouteGroupBuilder app)
        {
            app.MapPost("documentos/{id:guid}/copias/{copyId:guid}/revocar", async (Guid id, Guid copyId, [FromBody] ReasonRequest request, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.RevokeCopyAsync(id, copyId, request.Reason, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Revoca una copia controlada indicando el motivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
