using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class GetCopyWatermarkEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Marca de Agua";

        public static RouteGroupBuilder MapGetCopyWatermark(this RouteGroupBuilder app)
        {
            app.MapGet("documentos/{id:guid}/copias/{copyId:guid}/marca-agua", async (Guid id, Guid copyId, DocumentService documentService, CancellationToken ct) =>
            {
                var result = await documentService.GetWatermarkAsync(id, copyId, ct);
                return result.ToHttpResult();
            })
            .Produces<string>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Texto de la marca de agua COPIA CONTROLADA para el visor o el PDF")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
