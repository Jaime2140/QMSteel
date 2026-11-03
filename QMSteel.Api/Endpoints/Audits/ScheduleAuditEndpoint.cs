using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class ScheduleAuditEndpoint
    {
        private const string ENDPOINT_NAME = "Confirmar Programacion de Auditoria";

        public static RouteGroupBuilder MapScheduleAudit(this RouteGroupBuilder app)
        {
            app.MapPost("auditorias/{id:guid}/programar", async (Guid id, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.ScheduleAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<AuditDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Confirma la programación y genera la notificación a auditores y auditados")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
