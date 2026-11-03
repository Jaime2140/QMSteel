using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class StartAuditEndpoint
    {
        private const string ENDPOINT_NAME = "Iniciar Auditoria";

        public static RouteGroupBuilder MapStartAudit(this RouteGroupBuilder app)
        {
            app.MapPost("auditorias/{id:guid}/iniciar", async (Guid id, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.StartAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<AuditDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Marca la auditoría como en curso")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
