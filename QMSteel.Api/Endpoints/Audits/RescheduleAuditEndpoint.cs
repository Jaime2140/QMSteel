using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class RescheduleAuditEndpoint
    {
        private const string ENDPOINT_NAME = "Reprogramar Auditoria";

        public static RouteGroupBuilder MapRescheduleAudit(this RouteGroupBuilder app)
        {
            app.MapPut("auditorias/{id:guid}/fechas", async (Guid id, [FromBody] RescheduleAuditRequest request, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.RescheduleAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<AuditDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cambia las fechas de la auditoría")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
