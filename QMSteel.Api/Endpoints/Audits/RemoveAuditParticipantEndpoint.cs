using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Audits;

namespace QMSteel.Api.Endpoints.Audits
{
    public static class RemoveAuditParticipantEndpoint
    {
        private const string ENDPOINT_NAME = "Quitar Participante de Auditoria";

        public static RouteGroupBuilder MapRemoveAuditParticipant(this RouteGroupBuilder app)
        {
            app.MapDelete("auditorias/{id:guid}/participantes/{participantId:guid}", async (Guid id, Guid participantId, AuditService auditService, CancellationToken ct) =>
            {
                var result = await auditService.RemoveParticipantAsync(id, participantId, ct);
                return result.ToHttpResult();
            })
            .Produces<AuditDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Quita un participante de la auditoría")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
