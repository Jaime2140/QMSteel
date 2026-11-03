using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Entities.Personnel;
using QMSteel.Domain.Entities.Procedures;
using QMSteel.Domain.Entities.Welders;
using QMSteel.Domain.Execution;
using QMSteel.Domain.ValueObjects;
using QMSteel.Domain.Welding;

namespace QMSteel.Domain.Entities.Inspections
{
    // Reporte de inspección NDT (VT, PT, MT, UT, RT) o de recubrimientos (preparación, WFT, DFT).
    // Los bloqueos de RF-2, RF-3, RF-4 y RF-5 se aplican al capturar: no entra un soldador, WPS o equipo no elegible.
    public class Inspection : AggregateRoot
    {
        public string Folio { get; private set; } = default!;
        public InspectionMethod Method { get; private set; }
        public InspectionKind Kind => Method is InspectionMethod.SurfacePreparation or InspectionMethod.WFT or InspectionMethod.DFT
            ? InspectionKind.Coating
            : InspectionKind.Weld;

        public string ProjectName { get; private set; } = default!;
        public string? ClientName { get; private set; }
        public string PartNumber { get; private set; } = default!;
        public string? Location { get; private set; }
        public DateOnly InspectedOn { get; private set; }
        public Guid InspectorId { get; private set; }
        public string AcceptanceCriteria { get; private set; } = default!;
        public Guid? WpsId { get; private set; }
        public WeldingProcess? WpsProcess { get; private set; }
        public string? CleanlinessStandard { get; private set; }
        public string? Notes { get; private set; }

        public InspectionStatus Status { get; private set; } = InspectionStatus.Draft;
        public ElectronicSignature? SignedBy { get; private set; }
        public string? VoidReason { get; private set; }

        private readonly List<Guid> _equipmentIds = new();
        public IReadOnlyCollection<Guid> EquipmentIds => _equipmentIds;

        private readonly List<WeldInspectionItem> _weldItems = new();
        public IReadOnlyCollection<WeldInspectionItem> WeldItems => _weldItems;

        private readonly List<CoatingMeasurement> _coatingMeasurements = new();
        public IReadOnlyCollection<CoatingMeasurement> CoatingMeasurements => _coatingMeasurements;

        private readonly List<Guid> _photoFileIds = new();
        public IReadOnlyCollection<Guid> PhotoFileIds => _photoFileIds;

        private Inspection() { }

        public Inspection(Guid tenantId, string folio, InspectionMethod method, string projectName, string partNumber,
            DateOnly inspectedOn, Guid inspectorId, string acceptanceCriteria, string? clientName = null, string? location = null)
        {
            if (string.IsNullOrWhiteSpace(folio))
                throw new DomainException("INSPECTION_FOLIO_REQUIRED", "El folio de la inspección es obligatorio.");
            if (string.IsNullOrWhiteSpace(projectName))
                throw new DomainException("INSPECTION_PROJECT_REQUIRED", "La obra/proyecto es obligatoria.");
            if (string.IsNullOrWhiteSpace(partNumber))
                throw new DomainException("INSPECTION_PART_REQUIRED", "El elemento/marca es obligatorio.");
            if (string.IsNullOrWhiteSpace(acceptanceCriteria))
                throw new DomainException("INSPECTION_CRITERIA_REQUIRED", "Indica el criterio de aceptación (ej. AWS D1.1 Tabla 8.1).");
            if (inspectorId == Guid.Empty)
                throw new DomainException("INSPECTION_INSPECTOR_REQUIRED", "La inspección requiere un inspector.");

            TenantId = tenantId;
            Folio = folio.Trim().ToUpperInvariant();
            Method = method;
            ProjectName = projectName.Trim();
            PartNumber = partNumber.Trim().ToUpperInvariant();
            InspectedOn = inspectedOn;
            InspectorId = inspectorId;
            AcceptanceCriteria = acceptanceCriteria.Trim();
            ClientName = clientName?.Trim();
            Location = location?.Trim();
        }

        public bool IsNdt => Kind == InspectionKind.Weld;

        public NdtMethod? NdtMethod => IsNdt ? Enum.Parse<NdtMethod>(Method.ToString()) : null;

        public void AssignWps(Wps wps)
        {
            EnsureDraft();
            EnsureKind(InspectionKind.Weld);
            Ensure(wps.CheckEligibility());
            if (_weldItems.Any(i => i.Process != wps.Variables.Process))
                throw new DomainException("INSPECTION_WPS_PROCESS_MISMATCH", "El proceso del WPS no coincide con las soldaduras capturadas.");

            WpsId = wps.Id;
            WpsProcess = wps.Variables.Process;
        }

        public void AddEquipment(Equipment equipment)
        {
            EnsureDraft();
            Ensure(equipment.CheckEligibility(InspectedOn));
            if (!_equipmentIds.Contains(equipment.Id))
                _equipmentIds.Add(equipment.Id);
        }

        public void RemoveEquipment(Guid equipmentId)
        {
            EnsureDraft();
            _equipmentIds.Remove(equipmentId);
        }

        public WeldInspectionItem AddWeldResult(Welder welder, WeldingProcess process, string jointNumber, decimal inspectedLengthMm,
            decimal rejectedLengthMm, string? position = null, string? discontinuities = null, bool isTackWeld = false)
        {
            EnsureDraft();
            EnsureKind(InspectionKind.Weld);
            if (string.IsNullOrWhiteSpace(jointNumber))
                throw new DomainException("INSPECTION_JOINT_REQUIRED", "Indica la junta inspeccionada.");
            if (inspectedLengthMm <= 0)
                throw new DomainException("INSPECTION_LENGTH_INVALID", "La longitud inspeccionada debe ser mayor a cero.");
            if (rejectedLengthMm < 0 || rejectedLengthMm > inspectedLengthMm)
                throw new DomainException("INSPECTION_REJECTED_LENGTH_INVALID", "La longitud rechazada no puede exceder la inspeccionada.");
            if (rejectedLengthMm > 0 && string.IsNullOrWhiteSpace(discontinuities))
                throw new DomainException("INSPECTION_DISCONTINUITY_REQUIRED", "Describe la discontinuidad rechazada.");
            if (WpsProcess is { } wpsProcess && wpsProcess != process)
                throw new DomainException("INSPECTION_WPS_PROCESS_MISMATCH", "El proceso no coincide con el del WPS asignado.");

            Ensure(welder.CheckEligibility(InspectedOn, process, position, isTackWeld));

            var item = new WeldInspectionItem(jointNumber.Trim().ToUpperInvariant(), welder.Id, welder.StampNumber, process,
                position?.Trim().ToUpperInvariant(), isTackWeld, inspectedLengthMm, rejectedLengthMm, discontinuities?.Trim());
            _weldItems.Add(item);
            return item;
        }

        public CoatingMeasurement AddCoatingMeasurement(string location, decimal specifiedMinMicrons, decimal specifiedMaxMicrons, IEnumerable<decimal> readings)
        {
            EnsureDraft();
            EnsureKind(InspectionKind.Coating);
            if (string.IsNullOrWhiteSpace(location))
                throw new DomainException("INSPECTION_LOCATION_REQUIRED", "Indica la zona medida.");
            if (specifiedMinMicrons <= 0 || specifiedMaxMicrons < specifiedMinMicrons)
                throw new DomainException("INSPECTION_SPEC_RANGE_INVALID", "El rango especificado no es válido.");

            var values = readings.ToList();
            if (values.Count == 0 || values.Any(v => v <= 0))
                throw new DomainException("INSPECTION_READINGS_INVALID", "Captura lecturas mayores a cero.");

            var measurement = new CoatingMeasurement(location.Trim(), specifiedMinMicrons, specifiedMaxMicrons, values);
            _coatingMeasurements.Add(measurement);
            return measurement;
        }

        public void SetCleanlinessStandard(string? standard)
        {
            EnsureDraft();
            CleanlinessStandard = standard?.Trim().ToUpperInvariant();
        }

        public void RemoveItem(Guid itemId)
        {
            EnsureDraft();
            _weldItems.RemoveAll(i => i.Id == itemId);
            _coatingMeasurements.RemoveAll(m => m.Id == itemId);
        }

        public void AddPhoto(Guid fileId)
        {
            EnsureDraft();
            if (!_photoFileIds.Contains(fileId))
                _photoFileIds.Add(fileId);
        }

        public void SetNotes(string? notes)
        {
            EnsureDraft();
            Notes = notes?.Trim();
        }

        // Firma del inspector certificado; aplica el candado operativo de RF-5.
        public void Sign(Inspector inspector, ElectronicSignature signature)
        {
            EnsureDraft();
            if (inspector.Id != InspectorId)
                throw new DomainException("INSPECTION_WRONG_INSPECTOR", "Solo el inspector asignado puede firmar el reporte.");
            if (_weldItems.Count == 0 && _coatingMeasurements.Count == 0)
                throw new DomainException("INSPECTION_EMPTY", "La inspección no tiene resultados capturados.");
            if (Method == InspectionMethod.SurfacePreparation && CleanlinessStandard is null)
                throw new DomainException("INSPECTION_CLEANLINESS_REQUIRED", "Indica el grado de limpieza (ej. SSPC-SP10).");
            if (_equipmentIds.Count == 0 && Method != InspectionMethod.VT)
                throw new DomainException("INSPECTION_EQUIPMENT_REQUIRED", "Registra el equipo utilizado.");

            var signedOn = DateOnly.FromDateTime(signature.SignedAtUtc);
            if (NdtMethod is { } ndt)
                Ensure(inspector.CanSign(ndt, signedOn));
            else if (!inspector.IsActive)
                throw new DomainException("INSPECTOR_INACTIVE", "El inspector está inactivo.");

            Status = InspectionStatus.Signed;
            SignedBy = signature;

            var activities = _weldItems.Select(i => new WeldActivity(i.WelderId, i.Process)).Distinct().ToList();
            Raise(new InspectionSigned(Id, InspectedOn, activities, _equipmentIds.ToList(), signature.SignedAtUtc));

            var rejected = RejectedItemDescriptions();
            if (rejected.Count > 0)
                Raise(new InspectionRejectionsFound(Id, Folio, Method, rejected, signature.SignedAtUtc));
        }

        public void Void(string reason)
        {
            if (Status == InspectionStatus.Voided)
                throw new DomainException("INSPECTION_VOIDED", "La inspección ya fue anulada.");
            if (string.IsNullOrWhiteSpace(reason))
                throw new DomainException("INSPECTION_VOID_REASON_REQUIRED", "Indica el motivo de la anulación.");
            Status = InspectionStatus.Voided;
            VoidReason = reason.Trim();
        }

        public decimal InspectedLengthMm => _weldItems.Sum(i => i.InspectedLengthMm);
        public decimal RejectedLengthMm => _weldItems.Sum(i => i.RejectedLengthMm);

        // Tasa de rechazo por longitud (base del KPI de aceptación directa en taller).
        public decimal RejectionRatePercent => InspectedLengthMm == 0 ? 0 : decimal.Round(100m * RejectedLengthMm / InspectedLengthMm, 2);

        public bool HasRejections => _weldItems.Any(i => i.Result == ItemResult.Rejected)
            || _coatingMeasurements.Any(m => m.Result == ItemResult.Rejected);

        private List<string> RejectedItemDescriptions() =>
            _weldItems.Where(i => i.Result == ItemResult.Rejected)
                .Select(i => $"Junta {i.JointNumber} (estampa {i.WelderStamp}): {i.RejectedLengthMm} mm rechazados - {i.Discontinuities}")
                .Concat(_coatingMeasurements.Where(m => m.Result == ItemResult.Rejected)
                    .Select(m => $"{m.Location}: promedio {m.Average} µm, mínimo {m.Lowest} µm (especificado {m.SpecifiedMinMicrons}-{m.SpecifiedMaxMicrons} µm)"))
                .ToList();

        private static void Ensure(EligibilityResult eligibility)
        {
            if (!eligibility.IsEligible)
                throw new DomainException(eligibility.ReasonCode!, eligibility.Reason!);
        }

        private void EnsureKind(InspectionKind kind)
        {
            if (Kind != kind)
                throw new DomainException("INSPECTION_KIND_MISMATCH", "La captura no corresponde al método de inspección.");
        }

        private void EnsureDraft()
        {
            if (Status != InspectionStatus.Draft)
                throw new DomainException("INSPECTION_NOT_DRAFT", "La inspección ya fue firmada o anulada.");
        }
    }
}
