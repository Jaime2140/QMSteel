using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class AddAuditFindingEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Hallazgo";

        public static RouteGroupBuilder MapAddAuditFinding(this RouteGroupBuilder app)
        {
            app.MapPost("auditorias/{id:guid}/hallazgos", async (Guid id, [FromBody] AddFindingRequest request, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.AddFindingAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<AuditDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra un hallazgo; si es No Conformidad Mayor o Menor se abre la NC automáticamente")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
