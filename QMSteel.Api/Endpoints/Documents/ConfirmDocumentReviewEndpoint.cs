using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class ConfirmDocumentReviewEndpoint
    {
        private const string ENDPOINT_NAME = "Confirmar Revision Periodica";

        public static RouteGroupBuilder MapConfirmDocumentReview(this RouteGroupBuilder app)
        {
            app.MapPost("documentos/{id:guid}/confirmar-revision", async (Guid id, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.ConfirmReviewAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<DocumentDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Confirma sin cambios la vigencia de la revisión actual y recorre la próxima revisión")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
