using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class CompleteAuditEndpoint
    {
        private const string ENDPOINT_NAME = "Completar Auditoria";

        public static RouteGroupBuilder MapCompleteAudit(this RouteGroupBuilder app)
        {
            app.MapPost("auditorias/{id:guid}/completar", async (Guid id, [FromBody] CompleteAuditRequest request, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.CompleteAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<AuditDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cierra la auditoría con su informe y el resultado de requisitos")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
