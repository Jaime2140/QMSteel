using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Documents;

namespace QMSteel.Api.Endpoints.Documents
{
    public static class ListDocumentsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Documentos";

        public static RouteGroupBuilder MapListDocuments(this RouteGroupBuilder app)
        {
            app.MapGet("documentos", async ([FromQuery(Name = "soloActivos")] bool? onlyActive, DocumentService documentService, CancellationToken ct) =>
            {
                var response = await documentService.ListAsync(onlyActive, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<DocumentSummaryDto>>(StatusCodes.Status200OK)
            .WithDescription("Lista el catálogo de documentos controlados con su revisión vigente y estatus")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
