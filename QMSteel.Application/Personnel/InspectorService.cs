using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Personnel
{
    // RF-5: expediente de inspectores QC/NDT, certificaciones y examen de la vista.
    public sealed class InspectorService : ApplicationService
    {
        private readonly IInspectorRepository _inspectors;
        private readonly IUserRepository _users;

        public InspectorService(UseCaseContext context, IInspectorRepository inspectors, IUserRepository users) : base(context)
        {
            _inspectors = inspectors;
            _users = users;
        }

        public async Task<IReadOnlyList<InspectorSummaryDto>> ListAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            (await _inspectors.GetAllAsync(onlyActive, ct)).Select(i => InspectorSummaryDto.From(i, Today)).ToList();

        public Task<Result<InspectorDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_inspectors, id, Map, ct);

        // Expediente del inspector ligado al usuario en sesión (para saber si puede firmar).
        public async Task<Result<InspectorDto>> GetCurrentAsync(CancellationToken ct = default)
        {
            var inspector = await _inspectors.GetByUserIdAsync(UserId, ct);
            return inspector is null
                ? Result.Failure<InspectorDto>(ApplicationErrors.NotAnInspector)
                : Map(inspector);
        }

        public async Task<Result<InspectorDto>> CreateAsync(CreateInspectorRequest request, CancellationToken ct = default)
        {
            if (await _inspectors.ExistsByEmployeeNumberAsync(request.EmployeeNumber, ct))
                return Result.Failure<InspectorDto>(ApplicationErrors.Duplicate("INSPECTOR_EMPLOYEE_DUPLICATE", $"Ya existe un inspector con número de empleado {request.EmployeeNumber}."));

            if (request.UserId is { } userId)
            {
                var error = await CheckUserAsync(userId, null, ct);
                if (error is not null)
                    return Result.Failure<InspectorDto>(error);
            }

            var inspector = new Inspector(TenantId, request.EmployeeNumber, request.FullName, request.UserId);
            return await AddAsync(_inspectors, inspector, Map, ct);
        }

        public async Task<Result<InspectorDto>> LinkUserAsync(Guid id, Guid userId, CancellationToken ct = default)
        {
            var error = await CheckUserAsync(userId, id, ct);
            if (error is not null)
                return Result.Failure<InspectorDto>(error);

            return await ChangeAsync(_inspectors, id, i => i.LinkUser(userId), Map, ct);
        }

        public Task<Result<InspectorDto>> AddCertificationAsync(Guid id, AddCertificationRequest request, CancellationToken ct = default) =>
            ChangeAsync(_inspectors, id, i => i.AddCertification(request.Scheme, request.Method, request.Level, request.CertificateNumber,
                request.IssuingBody, request.IssuedOn, request.ExpiresOn, request.FileId), Map, ct);

        public Task<Result<InspectorDto>> RegisterVisionExamAsync(Guid id, RegisterVisionExamRequest request, CancellationToken ct = default) =>
            ChangeAsync(_inspectors, id, i => i.RegisterVisionExam(request.ExamDate, request.NearVisionPassed, request.ColorPerceptionPassed,
                request.ExaminerName, request.FileId), Map, ct);

        public Task<Result<InspectorDto>> DeactivateAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_inspectors, id, i => i.Deactivate(), Map, ct);

        public Task<Result<InspectorDto>> ActivateAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_inspectors, id, i => i.Activate(), Map, ct);

        private async Task<Error?> CheckUserAsync(Guid userId, Guid? inspectorId, CancellationToken ct)
        {
            if (await _users.GetByIdAsync(userId, ct) is null)
                return ApplicationErrors.NotFound<Domain.Entities.Users.User>(userId);

            var linked = await _inspectors.GetByUserIdAsync(userId, ct);
            return linked is not null && linked.Id != inspectorId
                ? ApplicationErrors.Duplicate("INSPECTOR_USER_DUPLICATE", "El usuario ya está ligado a otro inspector.")
                : null;
        }

        private InspectorDto Map(Inspector inspector) => InspectorDto.From(inspector, Today);
    }
}
