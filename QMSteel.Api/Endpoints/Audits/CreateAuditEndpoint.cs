using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class CreateAuditEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Auditoria";

        public static RouteGroupBuilder MapCreateAudit(this RouteGroupBuilder app)
        {
            app.MapPost("auditorias", async ([FromBody] CreateAuditRequest request, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.CreateAsync(request, ct);
                return result.ToCreatedResult(audit => $"/api/auditorias/{audit.Id}");
            })
            .Produces<AuditDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Programa una auditoría en el calendario anual")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
