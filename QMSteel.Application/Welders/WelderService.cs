using QMSteel.Application.Common;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Welders;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.Welding;

namespace QMSteel.Application.Welders
{
    // RF-3: catálogo de soldadores y punteadores, calificaciones (WPQR) y continuidad.
    public sealed class WelderService : ApplicationService
    {
        private readonly IWelderRepository _welders;
        private readonly IInspectionRepository _inspections;

        public WelderService(UseCaseContext context, IWelderRepository welders, IInspectionRepository inspections) : base(context)
        {
            _welders = welders;
            _inspections = inspections;
        }

        public async Task<IReadOnlyList<WelderSummaryDto>> ListAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            (await _welders.GetAllAsync(onlyActive, ct)).Select(w => WelderSummaryDto.From(w, Today)).ToList();

        // Lo que ve el inspector al capturar: solo soldadores que hoy pueden soldar el proceso y la posición.
        public async Task<IReadOnlyList<WelderSummaryDto>> ListEligibleAsync(WeldingProcess? process = null, string? position = null,
            bool isTackWeld = false, CancellationToken ct = default) =>
            (await _welders.GetAllAsync(true, ct))
                .Where(w => w.CheckEligibility(Today, process, position, isTackWeld).IsEligible)
                .Select(w => WelderSummaryDto.From(w, Today))
                .ToList();

        public async Task<WelderKpisDto> GetKpisAsync(CancellationToken ct = default)
        {
            var welders = await _welders.GetAllAsync(null, ct);
            var statuses = welders.Where(w => w.IsActive).Select(w => w.GetStatus(Today)).ToList();

            return new WelderKpisDto(
                Active: statuses.Count,
                Valid: statuses.Count(s => s == ResourceStatus.Valid),
                ExpiringSoon: statuses.Count(s => s == ResourceStatus.ExpiringSoon),
                ExpiredOrInactive: welders.Count(w => !w.IsActive) + statuses.Count(s => s is ResourceStatus.Expired or ResourceStatus.Blocked));
        }

        public Task<Result<WelderDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_welders, id, Map, ct);

        public async Task<Result<IReadOnlyList<WeldHistoryDto>>> GetHistoryAsync(Guid id, int take = 20, CancellationToken ct = default)
        {
            if (await _welders.GetByIdAsync(id, ct) is null)
                return NotFound<Welder, IReadOnlyList<WeldHistoryDto>>(id);

            var inspections = await _inspections.GetByWelderAsync(id, take, ct);
            return inspections
                .SelectMany(i => i.WeldItems.Where(item => item.WelderId == id).Select(item => new WeldHistoryDto(i.Id, i.Folio,
                    i.InspectedOn, i.Method, i.Status, i.ProjectName, item.JointNumber, item.Process, item.InspectedLengthMm,
                    item.RejectedLengthMm, item.Result)))
                .ToList();
        }

        public async Task<Result<WelderDto>> CreateAsync(CreateWelderRequest request, CancellationToken ct = default)
        {
            if (await _welders.ExistsByEmployeeNumberAsync(request.EmployeeNumber, ct))
                return Result.Failure<WelderDto>(ApplicationErrors.Duplicate("WELDER_EMPLOYEE_DUPLICATE", $"Ya existe un soldador con número de empleado {request.EmployeeNumber}."));
            if (await _welders.GetByStampNumberAsync(request.StampNumber, ct) is not null)
                return Result.Failure<WelderDto>(StampTaken(request.StampNumber));

            var welder = new Welder(TenantId, request.EmployeeNumber, request.FullName, request.StampNumber, request.Type);
            return await AddAsync(_welders, welder, Map, ct);
        }

        public async Task<Result<WelderDto>> UpdateAsync(Guid id, UpdateWelderRequest request, CancellationToken ct = default)
        {
            var owner = await _welders.GetByStampNumberAsync(request.StampNumber, ct);
            if (owner is not null && owner.Id != id)
                return Result.Failure<WelderDto>(StampTaken(request.StampNumber));

            return await ChangeAsync(_welders, id, w =>
            {
                w.UpdateInfo(request.FullName, request.StampNumber);
                w.ChangeType(request.Type);
            }, Map, ct);
        }

        public Task<Result<WelderDto>> AddQualificationAsync(Guid id, AddQualificationRequest request, CancellationToken ct = default) =>
            ChangeAsync(_welders, id, w => w.AddQualification(request.Process, request.TestPosition, request.QualifiedPositions,
                request.IssuedOn, request.ExpiresOn, request.WpsId, request.CertificateFileId), Map, ct);

        public Task<Result<WelderDto>> AttachCertificateAsync(Guid id, Guid qualificationId, Guid fileId, CancellationToken ct = default) =>
            ChangeAsync(_welders, id, w => FindQualification(w, qualificationId).AttachCertificate(fileId), Map, ct);

        public Task<Result<WelderDto>> RevokeQualificationAsync(Guid id, Guid qualificationId, string reason, CancellationToken ct = default) =>
            ChangeAsync(_welders, id, w => w.RevokeQualification(qualificationId, reason), Map, ct);

        public Task<Result<WelderDto>> DeactivateAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_welders, id, w => w.Deactivate(), Map, ct);

        public Task<Result<WelderDto>> ActivateAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_welders, id, w => w.Activate(), Map, ct);

        private static WelderQualification FindQualification(Welder welder, Guid qualificationId) =>
            welder.Qualifications.SingleOrDefault(q => q.Id == qualificationId)
                ?? throw new DomainException("WELDER_QUALIFICATION_NOT_FOUND", "La calificación no existe.");

        private static Error StampTaken(string stamp) =>
            ApplicationErrors.Duplicate("WELDER_STAMP_DUPLICATE", $"La estampa {stamp} ya está asignada a otro soldador.");

        private WelderDto Map(Welder welder) => WelderDto.From(welder, Today);
    }
}
