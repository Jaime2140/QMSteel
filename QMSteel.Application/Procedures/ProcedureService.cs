using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.Welding;

namespace QMSteel.Application.Procedures
{
    // RF-4: WPS y PQR, con validación de variables esenciales AWS D1.1.
    public sealed class ProcedureService : ApplicationService
    {
        private readonly IWpsRepository _wps;
        private readonly IPqrRepository _pqrs;

        public ProcedureService(UseCaseContext context, IWpsRepository wps, IPqrRepository pqrs) : base(context)
        {
            _wps = wps;
            _pqrs = pqrs;
        }

        // ---- PQR ----

        public async Task<IReadOnlyList<PqrDto>> ListPqrsAsync(CancellationToken ct = default) =>
            (await _pqrs.GetAllAsync(ct)).Select(PqrDto.From).ToList();

        public Task<Result<PqrDto>> GetPqrAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_pqrs, id, PqrDto.From, ct);

        public async Task<Result<PqrDto>> CreatePqrAsync(CreatePqrRequest request, CancellationToken ct = default)
        {
            if (await _pqrs.GetByNumberAsync(request.Number, ct) is not null)
                return Result.Failure<PqrDto>(ApplicationErrors.Duplicate("PQR_NUMBER_DUPLICATE", $"Ya existe el PQR {request.Number}."));

            var pqr = new Pqr(TenantId, request.Number, request.Variables.ToDomain(), request.TestedOn, request.TestingLaboratory, request.FileId);
            return await AddAsync(_pqrs, pqr, PqrDto.From, ct);
        }

        public Task<Result<PqrDto>> UpdatePqrVariablesAsync(Guid id, EssentialVariablesDto variables, CancellationToken ct = default) =>
            ChangeAsync(_pqrs, id, p => p.UpdateVariables(variables.ToDomain()), PqrDto.From, ct);

        public Task<Result<PqrDto>> AttachPqrFileAsync(Guid id, Guid fileId, CancellationToken ct = default) =>
            ChangeAsync(_pqrs, id, p => p.AttachFile(fileId), PqrDto.From, ct);

        public Task<Result<PqrDto>> ApprovePqrAsync(Guid id, CancellationToken ct = default) =>
            SignAndChangeAsync(_pqrs, id, "Aprobó", true, (p, signature) => p.Approve(signature), PqrDto.From, ct);

        public Task<Result<PqrDto>> MakePqrObsoleteAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_pqrs, id, p => p.MakeObsolete(), PqrDto.From, ct);

        // ---- WPS ----

        public async Task<IReadOnlyList<WpsDto>> ListWpsAsync(WeldingProcess? process = null, WpsStatus? status = null,
            JointType? jointType = null, CancellationToken ct = default) =>
            (await _wps.GetAllAsync(process, status, ct))
                .Where(w => jointType is null || w.JointType == jointType)
                .Select(WpsDto.From)
                .ToList();

        // Matriz WPS & PQR: cada WPS con sus PQR de respaldo y las discrepancias que encuentre la validación.
        public async Task<IReadOnlyList<WpsMatrixRowDto>> GetMatrixAsync(WeldingProcess? process = null, CancellationToken ct = default)
        {
            var wpsList = await _wps.GetAllAsync(process, null, ct);
            var pqrs = (await _pqrs.GetByIdsAsync(wpsList.SelectMany(w => w.SupportingPqrIds), ct)).ToDictionary(p => p.Id);

            return wpsList.Select(w =>
            {
                var supporting = w.SupportingPqrIds.Where(pqrs.ContainsKey).Select(id => pqrs[id]).ToList();
                var discrepancies = w.IsPrequalified || supporting.Count == 0
                    ? Array.Empty<DiscrepancyDto>()
                    : WpsPqrValidator.Validate(w.Variables, supporting).Select(DiscrepancyDto.From).ToArray();

                return new WpsMatrixRowDto(w.Id, w.Label, w.Variables.Process, w.JointType, RangeDto.From(w.Variables.ThicknessMm),
                    w.Status, w.IsPrequalified, supporting.Select(p => p.Number).ToList(), discrepancies);
            }).ToList();
        }

        public Task<Result<WpsDto>> GetWpsAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_wps, id, WpsDto.From, ct);

        public async Task<Result<WpsDto>> CreateWpsAsync(CreateWpsRequest request, CancellationToken ct = default)
        {
            if (await _wps.GetByNumberAsync(request.Number, ct) is not null)
                return Result.Failure<WpsDto>(ApplicationErrors.Duplicate("WPS_NUMBER_DUPLICATE", $"Ya existe el WPS {request.Number}."));

            var wps = new Wps(TenantId, request.Number, request.JointType, request.WeldType, request.Variables.ToDomain(), request.IsPrequalified);
            if (request.FileId is { } fileId)
                wps.AttachFile(fileId);

            return await AddAsync(_wps, wps, WpsDto.From, ct);
        }

        public Task<Result<WpsDto>> UpdateWpsAsync(Guid id, UpdateWpsRequest request, CancellationToken ct = default) =>
            ChangeAsync(_wps, id, w => w.Update(request.JointType, request.WeldType, request.Variables.ToDomain(), request.IsPrequalified),
                WpsDto.From, ct);

        public Task<Result<WpsDto>> AttachWpsFileAsync(Guid id, Guid fileId, CancellationToken ct = default) =>
            ChangeAsync(_wps, id, w => w.AttachFile(fileId), WpsDto.From, ct);

        public async Task<Result<WpsDto>> LinkPqrAsync(Guid id, Guid pqrId, CancellationToken ct = default)
        {
            if (await _pqrs.GetByIdAsync(pqrId, ct) is null)
                return NotFound<Pqr, WpsDto>(pqrId);

            return await ChangeAsync(_wps, id, w => w.LinkPqr(pqrId), WpsDto.From, ct);
        }

        public Task<Result<WpsDto>> UnlinkPqrAsync(Guid id, Guid pqrId, CancellationToken ct = default) =>
            ChangeAsync(_wps, id, w => w.UnlinkPqr(pqrId), WpsDto.From, ct);

        public Task<Result<WpsDto>> SubmitForReviewAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_wps, id, w => w.SubmitForReview(), WpsDto.From, ct);

        public Task<Result<WpsDto>> ReturnToDraftAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_wps, id, w => w.ReturnToDraft(), WpsDto.From, ct);

        // Si el PQR no respalda los rangos del WPS no se aprueba: el error lista cada variable fuera de rango.
        public async Task<Result<WpsDto>> ApproveWpsAsync(Guid id, CancellationToken ct = default)
        {
            var wps = await _wps.GetByIdAsync(id, ct);
            if (wps is null)
                return NotFound<Wps, WpsDto>(id);

            var signature = await Context.Signatures.SignAsync("Aprobó", requireApprover: true, ct);
            if (signature.IsFailure)
                return Result.Failure<WpsDto>(signature.Error);

            var pqrs = await _pqrs.GetByIdsAsync(wps.SupportingPqrIds, ct);
            var discrepancies = wps.Approve(signature.Value, Today, pqrs);
            if (discrepancies.Count > 0)
                return Result.Failure<WpsDto>(Error.Validation("WPS_PQR_DISCREPANCIES",
                    "El PQR no respalda las variables del WPS: " + string.Join("; ",
                        discrepancies.Select(d => $"{d.Variable} (WPS {d.WpsValue}, PQR {d.PqrValue})"))));

            _wps.Update(wps);
            await SaveAsync(ct);
            return WpsDto.From(wps);
        }

        public Task<Result<WpsDto>> StartNewRevisionAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_wps, id, w => w.StartNewRevision(), WpsDto.From, ct);

        public Task<Result<WpsDto>> MakeWpsObsoleteAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_wps, id, w => w.MakeObsolete(), WpsDto.From, ct);
    }
}
