using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Entities.Welders;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Inspections
{
    // RF-9: captura de inspecciones NDT y de recubrimiento. Al firmar, el manejador de eventos registra la
    // continuidad de los soldadores y el uso de los equipos.
    public sealed class InspectionService : ApplicationService
    {
        public const string FolioPrefix = "INS";

        private readonly IInspectionRepository _inspections;
        private readonly IInspectorRepository _inspectors;
        private readonly IWelderRepository _welders;
        private readonly IWpsRepository _wps;
        private readonly IEquipmentRepository _equipments;
        private readonly IFolioGenerator _folios;

        public InspectionService(UseCaseContext context, IInspectionRepository inspections, IInspectorRepository inspectors,
            IWelderRepository welders, IWpsRepository wps, IEquipmentRepository equipments, IFolioGenerator folios) : base(context)
        {
            _inspections = inspections;
            _inspectors = inspectors;
            _welders = welders;
            _wps = wps;
            _equipments = equipments;
            _folios = folios;
        }

        public Task<Result<InspectionDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_inspections, id, InspectionDto.From, ct);

        public async Task<IReadOnlyList<InspectionSummaryDto>> ListByProjectAsync(string projectName, CancellationToken ct = default) =>
            (await _inspections.GetByProjectAsync(projectName, ct)).Select(InspectionSummaryDto.From).ToList();

        public async Task<IReadOnlyList<InspectionSummaryDto>> ListSignedBetweenAsync(DateOnly from, DateOnly to, CancellationToken ct = default) =>
            (await _inspections.GetSignedBetweenAsync(from, to, ct)).Select(InspectionSummaryDto.From).ToList();

        public async Task<InspectionKpisDto> GetKpisAsync(DateOnly from, DateOnly to, CancellationToken ct = default)
        {
            var inspections = await _inspections.GetSignedBetweenAsync(from, to, ct);

            var byMethod = inspections.GroupBy(i => i.Method)
                .OrderBy(g => g.Key)
                .Select(g => new MethodKpiDto(g.Key, g.Count(), g.Count(i => i.HasRejections),
                    Meters(g.Sum(i => i.InspectedLengthMm)), Meters(g.Sum(i => i.RejectedLengthMm))))
                .ToList();

            var trend = inspections.GroupBy(i => (i.InspectedOn.Year, i.InspectedOn.Month))
                .OrderBy(g => g.Key)
                .Select(g => new MonthlyTrendDto(g.Key.Year, g.Key.Month, g.Count(), QualityKpis.RejectionPercentage(g)))
                .ToList();

            return new InspectionKpisDto(inspections.Count, Meters(inspections.Sum(i => i.InspectedLengthMm)),
                Meters(inspections.Sum(i => i.RejectedLengthMm)), QualityKpis.DirectAcceptancePercentage(inspections),
                QualityKpis.RejectionPercentage(inspections), byMethod, trend);
        }

        public async Task<Result<InspectionDto>> CreateAsync(CreateInspectionRequest request, CancellationToken ct = default)
        {
            var inspector = request.InspectorId is { } inspectorId
                ? await _inspectors.GetByIdAsync(inspectorId, ct)
                : await _inspectors.GetByUserIdAsync(UserId, ct);
            if (inspector is null)
                return request.InspectorId is { } missing
                    ? NotFound<Inspector, InspectionDto>(missing)
                    : Result.Failure<InspectionDto>(ApplicationErrors.NotAnInspector);

            var inspectedOn = request.InspectedOn ?? Today;
            var folio = await _folios.NextAsync(FolioPrefix, inspectedOn.Year, ct);
            var inspection = new Inspection(TenantId, folio, request.Method, request.ProjectName, request.PartNumber, inspectedOn,
                inspector.Id, request.AcceptanceCriteria, request.ClientName, request.Location);

            return await AddAsync(_inspections, inspection, InspectionDto.From, ct);
        }

        // WPS en revisión u obsoleto no se puede seleccionar (RF-4); lo bloquea el dominio.
        public async Task<Result<InspectionDto>> AssignWpsAsync(Guid id, Guid wpsId, CancellationToken ct = default)
        {
            var wps = await _wps.GetByIdAsync(wpsId, ct);
            return wps is null
                ? NotFound<Wps, InspectionDto>(wpsId)
                : await ChangeAsync(_inspections, id, i => i.AssignWps(wps), InspectionDto.From, ct);
        }

        public async Task<Result<InspectionDto>> AddEquipmentAsync(Guid id, Guid equipmentId, CancellationToken ct = default)
        {
            var equipment = await _equipments.GetByIdAsync(equipmentId, ct);
            return equipment is null
                ? NotFound<Equipment, InspectionDto>(equipmentId)
                : await ChangeAsync(_inspections, id, i => i.AddEquipment(equipment), InspectionDto.From, ct);
        }

        public Task<Result<InspectionDto>> RemoveEquipmentAsync(Guid id, Guid equipmentId, CancellationToken ct = default) =>
            ChangeAsync(_inspections, id, i => i.RemoveEquipment(equipmentId), InspectionDto.From, ct);

        // Soldador vencido, sin continuidad o fuera de su proceso/posición: el dominio lo rechaza con el motivo para el modal rojo.
        public async Task<Result<InspectionDto>> AddWeldResultAsync(Guid id, AddWeldResultRequest request, CancellationToken ct = default)
        {
            var welder = await _welders.GetByIdAsync(request.WelderId, ct);
            return welder is null
                ? NotFound<Welder, InspectionDto>(request.WelderId)
                : await ChangeAsync(_inspections, id, i => i.AddWeldResult(welder, request.Process, request.JointNumber,
                    request.InspectedLengthMm, request.RejectedLengthMm, request.Position, request.Discontinuities, request.IsTackWeld),
                    InspectionDto.From, ct);
        }

        public Task<Result<InspectionDto>> AddCoatingMeasurementAsync(Guid id, AddCoatingMeasurementRequest request, CancellationToken ct = default) =>
            ChangeAsync(_inspections, id, i => i.AddCoatingMeasurement(request.Location, request.SpecifiedMinMicrons,
                request.SpecifiedMaxMicrons, request.Readings), InspectionDto.From, ct);

        public Task<Result<InspectionDto>> SetCleanlinessStandardAsync(Guid id, string? standard, CancellationToken ct = default) =>
            ChangeAsync(_inspections, id, i => i.SetCleanlinessStandard(standard), InspectionDto.From, ct);

        public Task<Result<InspectionDto>> RemoveItemAsync(Guid id, Guid itemId, CancellationToken ct = default) =>
            ChangeAsync(_inspections, id, i => i.RemoveItem(itemId), InspectionDto.From, ct);

        public Task<Result<InspectionDto>> AddPhotoAsync(Guid id, Guid fileId, CancellationToken ct = default) =>
            ChangeAsync(_inspections, id, i => i.AddPhoto(fileId), InspectionDto.From, ct);

        public Task<Result<InspectionDto>> SetNotesAsync(Guid id, string? notes, CancellationToken ct = default) =>
            ChangeAsync(_inspections, id, i => i.SetNotes(notes), InspectionDto.From, ct);

        // Firma del inspector en sesión; aplica el candado operativo de RF-5 (certificación y examen de la vista).
        public async Task<Result<InspectionDto>> SignAsync(Guid id, CancellationToken ct = default)
        {
            var inspector = await _inspectors.GetByUserIdAsync(UserId, ct);
            if (inspector is null)
                return Result.Failure<InspectionDto>(ApplicationErrors.NotAnInspector);

            return await SignAndChangeAsync(_inspections, id, "Inspeccionó", false,
                (i, signature) => i.Sign(inspector, signature), InspectionDto.From, ct);
        }

        public Task<Result<InspectionDto>> VoidAsync(Guid id, string reason, CancellationToken ct = default) =>
            ChangeAsync(_inspections, id, i => i.Void(reason), InspectionDto.From, ct);

        private static decimal Meters(decimal millimeters) => decimal.Round(millimeters / 1000m, 2);
    }
}
