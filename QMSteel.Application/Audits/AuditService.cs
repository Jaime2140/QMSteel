using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Audits;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Audits
{
    // RF-8: calendario de auditorías y hallazgos. Un hallazgo de NC mayor/menor abre la NC automáticamente (manejador de evento).
    public sealed class AuditService : ApplicationService
    {
        public const string FolioPrefix = "AUD";

        private readonly IAuditRepository _audits;
        private readonly IFolioGenerator _folios;

        public AuditService(UseCaseContext context, IAuditRepository audits, IFolioGenerator folios) : base(context)
        {
            _audits = audits;
            _folios = folios;
        }

        public async Task<IReadOnlyList<AuditSummaryDto>> ListByYearAsync(int year, CancellationToken ct = default) =>
            (await _audits.GetByYearAsync(year, ct)).Select(AuditSummaryDto.From).ToList();

        public Task<Result<AuditDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_audits, id, AuditDto.From, ct);

        public async Task<Result<AuditDto>> CreateAsync(CreateAuditRequest request, CancellationToken ct = default)
        {
            var folio = await _folios.NextAsync(FolioPrefix, request.PlannedStart.Year, ct);
            var audit = new Audit(TenantId, folio, request.Type, request.Standard, request.Scope, request.PlannedStart, request.PlannedEnd);
            return await AddAsync(_audits, audit, AuditDto.From, ct);
        }

        public Task<Result<AuditDto>> AddParticipantAsync(Guid id, AddParticipantRequest request, CancellationToken ct = default) =>
            ChangeAsync(_audits, id, a => a.AddParticipant(request.Name, request.Email, request.Role, request.UserId), AuditDto.From, ct);

        public Task<Result<AuditDto>> RemoveParticipantAsync(Guid id, Guid participantId, CancellationToken ct = default) =>
            ChangeAsync(_audits, id, a => a.RemoveParticipant(participantId), AuditDto.From, ct);

        // Confirma la programación; el evento resultante es el que dispara el correo a auditores y auditados.
        public Task<Result<AuditDto>> ScheduleAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_audits, id, a => a.Schedule(UtcNow), AuditDto.From, ct);

        public Task<Result<AuditDto>> RescheduleAsync(Guid id, RescheduleAuditRequest request, CancellationToken ct = default) =>
            ChangeAsync(_audits, id, a => a.Reschedule(request.PlannedStart, request.PlannedEnd, UtcNow), AuditDto.From, ct);

        public Task<Result<AuditDto>> StartAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_audits, id, a => a.Start(), AuditDto.From, ct);

        public Task<Result<AuditDto>> AddFindingAsync(Guid id, AddFindingRequest request, CancellationToken ct = default) =>
            ChangeAsync(_audits, id, a => a.AddFinding(request.Clause, request.Description, request.Classification, UtcNow), AuditDto.From, ct);

        public Task<Result<AuditDto>> CompleteAsync(Guid id, CompleteAuditRequest request, CancellationToken ct = default) =>
            ChangeAsync(_audits, id, a => a.Complete(request.ReportFileId, request.RequirementsEvaluated, request.RequirementsCompliant),
                AuditDto.From, ct);

        public Task<Result<AuditDto>> CancelAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_audits, id, a => a.Cancel(), AuditDto.From, ct);
    }
}
