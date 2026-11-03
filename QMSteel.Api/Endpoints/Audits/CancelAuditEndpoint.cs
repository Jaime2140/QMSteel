using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class CancelAuditEndpoint
    {
        private const string ENDPOINT_NAME = "Cancelar Auditoria";

        public static RouteGroupBuilder MapCancelAudit(this RouteGroupBuilder app)
        {
            app.MapPost("auditorias/{id:guid}/cancelar", async (Guid id, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.CancelAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<AuditDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cancela la auditoría")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
