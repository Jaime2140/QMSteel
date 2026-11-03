using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class AddAuditParticipantEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Participante de Auditoria";

        public static RouteGroupBuilder MapAddAuditParticipant(this RouteGroupBuilder app)
        {
            app.MapPost("auditorias/{id:guid}/participantes", async (Guid id, [FromBody] AddParticipantRequest request, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.AddParticipantAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<AuditDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Agrega un auditor o auditado")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
