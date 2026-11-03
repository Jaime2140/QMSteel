using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class GetAuditEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Auditoria";

        public static RouteGroupBuilder MapGetAudit(this RouteGroupBuilder app)
        {
            app.MapGet("auditorias/{id:guid}", async (Guid id, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<AuditDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Detalle de la auditoría con participantes y hallazgos")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
