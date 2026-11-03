using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Application.NonConformities
{
    // RF-7: registro, análisis de causa raíz, acciones correctivas y cierre de NC.
    public sealed class NonConformityService : ApplicationService
    {
        private readonly INonConformityRepository _nonConformities;
        private readonly IUserRepository _users;
        private readonly IFolioGenerator _folios;

        public NonConformityService(UseCaseContext context, INonConformityRepository nonConformities, IUserRepository users,
            IFolioGenerator folios) : base(context)
        {
            _nonConformities = nonConformities;
            _users = users;
            _folios = folios;
        }

        public async Task<IReadOnlyList<NonConformitySummaryDto>> ListOpenAsync(CancellationToken ct = default) =>
            (await _nonConformities.GetOpenAsync(ct)).Select(n => NonConformitySummaryDto.From(n, Today)).ToList();

        public async Task<IReadOnlyList<NonConformitySummaryDto>> ListDetectedBetweenAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
            (await _nonConformities.GetDetectedBetweenAsync(from, to, ct)).Select(n => NonConformitySummaryDto.From(n, Today)).ToList();

        public async Task<NonConformityMonitorDto> GetMonitorAsync(DateOnly from, DateOnly to, Currency currency = Currency.USD,
            CancellationToken ct = default)
        {
            var open = await _nonConformities.GetOpenAsync(ct);
            var period = await _nonConformities.GetDetectedBetweenAsync(from, to, ct);

            return new NonConformityMonitorDto(open.Count, open.Count(n => n.IsOverdueOn(Today)),
                QualityKpis.ClosureEffectivenessPercentage(period), QualityKpis.AverageDaysToClose(period),
                MoneyDto.From(QualityKpis.CostOfPoorQuality(period, currency))!);
        }

        public Task<Result<NonConformityDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_nonConformities, id, Map, ct);

        public async Task<Result<NonConformityDto>> OpenAsync(OpenNonConformityRequest request, CancellationToken ct = default)
        {
            var detectedOn = request.DetectedOn ?? Today;
            var folio = await _folios.NextAsync(NonConformityFolio.Prefix, detectedOn.Year, ct);
            var nonConformity = new NonConformity(TenantId, folio, request.Source, request.Criticality, request.DefectType,
                request.Description, detectedOn, UserId, UtcNow, request.SourceReferenceId, request.SupplierId, request.ProjectName,
                request.ClientName, request.PartNumber);

            foreach (var fileId in request.EvidenceFileIds ?? [])
                nonConformity.AddEvidence(fileId);

            return await AddAsync(_nonConformities, nonConformity, Map, ct);
        }

        public Task<Result<NonConformityDto>> UpdateAsync(Guid id, UpdateNonConformityRequest request, CancellationToken ct = default) =>
            ChangeAsync(_nonConformities, id, n => n.UpdateDetails(request.Criticality, request.DefectType, request.Description,
                request.ProjectName, request.ClientName, request.PartNumber), Map, ct);

        public Task<Result<NonConformityDto>> AddEvidenceAsync(Guid id, Guid fileId, CancellationToken ct = default) =>
            ChangeAsync(_nonConformities, id, n => n.AddEvidence(fileId), Map, ct);

        public Task<Result<NonConformityDto>> SetDispositionAsync(Guid id, Disposition disposition, CancellationToken ct = default) =>
            ChangeAsync(_nonConformities, id, n => n.SetDisposition(disposition), Map, ct);

        // Comisión de atención de la acción correctiva; solo usuarios de la planta.
        public async Task<Result<NonConformityDto>> AssignCommitteeAsync(Guid id, IReadOnlyList<Guid> memberUserIds, CancellationToken ct = default)
        {
            foreach (var userId in memberUserIds.Distinct())
                if (await _users.GetByIdAsync(userId, ct) is null)
                    return NotFound<Domain.Entities.Users.User, NonConformityDto>(userId);

            return await ChangeAsync(_nonConformities, id, n => n.AssignCommittee(memberUserIds, UtcNow), Map, ct);
        }

        public Task<Result<NonConformityDto>> RecordRootCauseAsync(Guid id, RootCauseRequest request, CancellationToken ct = default) =>
            ChangeAsync(_nonConformities, id, n => n.RecordRootCauseAnalysis(request.Whys, request.RootCause, request.RequiresCorrectiveAction),
                Map, ct);

        public Task<Result<NonConformityDto>> AddActionAsync(Guid id, AddActionRequest request, CancellationToken ct = default) =>
            ChangeAsync(_nonConformities, id, n => n.AddAction(request.Type, request.Description, request.ResponsibleName, request.DueOn,
                request.ResponsibleUserId), Map, ct);

        public Task<Result<NonConformityDto>> CompleteActionAsync(Guid id, Guid actionId, CompleteActionRequest request,
            CancellationToken ct = default) =>
            ChangeAsync(_nonConformities, id, n => n.CompleteAction(actionId, request.CompletedOn ?? Today, request.Notes,
                request.EvidenceFileId), Map, ct);

        public Task<Result<NonConformityDto>> RescheduleActionAsync(Guid id, Guid actionId, DateOnly dueOn, CancellationToken ct = default) =>
            ChangeAsync(_nonConformities, id, n => n.RescheduleAction(actionId, dueOn), Map, ct);

        public Task<Result<NonConformityDto>> RegisterCostAsync(Guid id, MoneyDto cost, CancellationToken ct = default) =>
            ChangeAsync(_nonConformities, id, n => n.RegisterCost(cost.ToDomain()), Map, ct);

        public Task<Result<NonConformityDto>> CloseAsync(Guid id, CloseNonConformityRequest request, CancellationToken ct = default) =>
            SignAndChangeAsync(_nonConformities, id, "Cerró", true,
                (n, signature) => n.Close(signature, request.ClosedOn ?? Today, request.WasEffective), Map, ct);

        public Task<Result<NonConformityDto>> CancelAsync(Guid id, string reason, CancellationToken ct = default) =>
            ChangeAsync(_nonConformities, id, n => n.Cancel(reason), Map, ct);

        private NonConformityDto Map(NonConformity nonConformity) => NonConformityDto.From(nonConformity, Today);
    }
}
