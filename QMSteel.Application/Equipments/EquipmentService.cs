using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Equipments
{
    // RF-2: calibración externa, verificación diaria/periódica y frecuencia de uso de equipos.
    public sealed class EquipmentService : ApplicationService
    {
        private readonly IEquipmentRepository _equipments;

        public EquipmentService(UseCaseContext context, IEquipmentRepository equipments) : base(context)
        {
            _equipments = equipments;
        }

        public async Task<IReadOnlyList<EquipmentSummaryDto>> ListAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            (await _equipments.GetAllAsync(onlyActive, ct)).Select(e => EquipmentSummaryDto.From(e, Today)).ToList();

        // Solo los equipos que hoy pueden registrarse en una inspección.
        public async Task<IReadOnlyList<EquipmentSummaryDto>> ListEligibleAsync(CancellationToken ct = default) =>
            (await _equipments.GetAllAsync(true, ct))
                .Where(e => e.CheckEligibility(Today).IsEligible)
                .Select(e => EquipmentSummaryDto.From(e, Today))
                .ToList();

        public async Task<IReadOnlyList<EquipmentSummaryDto>> ListCalibrationDueAsync(int withinDays = Equipment.ExpiringSoonDays,
            CancellationToken ct = default) =>
            (await _equipments.GetCalibrationDueBeforeAsync(Today.AddDays(withinDays + 1), ct))
                .Select(e => EquipmentSummaryDto.From(e, Today))
                .ToList();

        public Task<Result<EquipmentDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_equipments, id, Map, ct);

        public async Task<Result<EquipmentDto>> CreateAsync(CreateEquipmentRequest request, CancellationToken ct = default)
        {
            if (await _equipments.GetByCodeAsync(request.Code, ct) is not null)
                return Result.Failure<EquipmentDto>(ApplicationErrors.Duplicate("EQUIPMENT_CODE_DUPLICATE", $"Ya existe un equipo con código {request.Code}."));

            var equipment = new Equipment(TenantId, request.Code, request.Name, request.Category, request.CalibrationIntervalMonths,
                request.VerificationFrequencyDays, request.Brand, request.Model, request.SerialNumber);
            equipment.SetLocation(request.Location);

            return await AddAsync(_equipments, equipment, Map, ct);
        }

        public Task<Result<EquipmentDto>> UpdateAsync(Guid id, UpdateEquipmentRequest request, CancellationToken ct = default) =>
            ChangeAsync(_equipments, id, e =>
            {
                e.SetCalibrationPolicy(request.CalibrationIntervalMonths, request.VerificationFrequencyDays);
                e.SetLocation(request.Location);
                e.SetPhoto(request.PhotoFileId);
            }, Map, ct);

        public Task<Result<EquipmentDto>> RegisterCalibrationAsync(Guid id, RegisterCalibrationRequest request, CancellationToken ct = default) =>
            ChangeAsync(_equipments, id, e => e.RegisterCalibration(request.CalibratedOn, request.LaboratoryName, request.AccreditationNumber,
                request.CertificateNumber, request.Passed, request.CertificateFileId, request.SupplierId, request.DueOn, UtcNow), Map, ct);

        public Task<Result<EquipmentDto>> AttachCalibrationCertificateAsync(Guid id, Guid calibrationId, Guid fileId, CancellationToken ct = default) =>
            ChangeAsync(_equipments, id, e => e.AttachCalibrationCertificate(calibrationId, fileId), Map, ct);

        public Task<Result<EquipmentDto>> RegisterVerificationAsync(Guid id, RegisterVerificationRequest request, CancellationToken ct = default) =>
            SignAndChangeAsync(_equipments, id, "Verificó", false,
                (e, signature) => e.RegisterVerification(request.PerformedOn ?? Today, request.Passed, signature, request.Notes), Map, ct);

        public Task<Result<EquipmentDto>> TakeOutOfServiceAsync(Guid id, string reason, CancellationToken ct = default) =>
            ChangeAsync(_equipments, id, e => e.TakeOutOfService(reason), Map, ct);

        public Task<Result<EquipmentDto>> ReturnToServiceAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_equipments, id, e => e.ReturnToService(), Map, ct);

        public Task<Result<EquipmentDto>> RetireAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_equipments, id, e => e.Retire(), Map, ct);

        private EquipmentDto Map(Equipment equipment) => EquipmentDto.From(equipment, Today);
    }
}
