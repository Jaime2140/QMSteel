using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Welding;

namespace QMSteel.Application.Inspections
{
    // Si no se indica inspector, se asigna el inspector ligado al usuario que captura.
    public sealed record CreateInspectionRequest(InspectionMethod Method, string ProjectName, string PartNumber, string AcceptanceCriteria,
        DateOnly? InspectedOn = null, Guid? InspectorId = null, string? ClientName = null, string? Location = null);

    public sealed record AddWeldResultRequest(Guid WelderId, WeldingProcess Process, string JointNumber, decimal InspectedLengthMm,
        decimal RejectedLengthMm, string? Position = null, string? Discontinuities = null, bool IsTackWeld = false);

    public sealed record AddCoatingMeasurementRequest(string Location, decimal SpecifiedMinMicrons, decimal SpecifiedMaxMicrons,
        IReadOnlyList<decimal> Readings);

    public sealed record WeldItemDto(Guid Id, string JointNumber, Guid WelderId, string WelderStamp, WeldingProcess Process, string? Position,
        bool IsTackWeld, decimal InspectedLengthMm, decimal RejectedLengthMm, string? Discontinuities, ItemResult Result)
    {
        public static WeldItemDto From(WeldInspectionItem i) => new(i.Id, i.JointNumber, i.WelderId, i.WelderStamp, i.Process, i.Position,
            i.IsTackWeld, i.InspectedLengthMm, i.RejectedLengthMm, i.Discontinuities, i.Result);
    }

    public sealed record CoatingMeasurementDto(Guid Id, string Location, decimal SpecifiedMinMicrons, decimal SpecifiedMaxMicrons,
        IReadOnlyList<decimal> Readings, decimal Average, decimal Lowest, decimal Highest, ItemResult Result)
    {
        public static CoatingMeasurementDto From(CoatingMeasurement m) => new(m.Id, m.Location, m.SpecifiedMinMicrons,
            m.SpecifiedMaxMicrons, m.Readings.ToList(), m.Average, m.Lowest, m.Highest, m.Result);
    }

    public sealed record InspectionSummaryDto(Guid Id, string Folio, InspectionMethod Method, InspectionKind Kind, string ProjectName,
        string PartNumber, DateOnly InspectedOn, InspectionStatus Status, bool HasRejections, decimal RejectionRatePercent)
    {
        public static InspectionSummaryDto From(Inspection i) => new(i.Id, i.Folio, i.Method, i.Kind, i.ProjectName, i.PartNumber,
            i.InspectedOn, i.Status, i.HasRejections, i.RejectionRatePercent);
    }

    public sealed record InspectionDto(Guid Id, string Folio, InspectionMethod Method, InspectionKind Kind, string ProjectName,
        string? ClientName, string PartNumber, string? Location, DateOnly InspectedOn, Guid InspectorId, string AcceptanceCriteria,
        Guid? WpsId, string? CleanlinessStandard, string? Notes, InspectionStatus Status, SignatureDto? SignedBy, string? VoidReason,
        IReadOnlyList<Guid> EquipmentIds, IReadOnlyList<Guid> PhotoFileIds, IReadOnlyList<WeldItemDto> WeldItems,
        IReadOnlyList<CoatingMeasurementDto> CoatingMeasurements, decimal InspectedLengthMm, decimal RejectedLengthMm,
        decimal RejectionRatePercent, bool HasRejections)
    {
        public static InspectionDto From(Inspection i) => new(i.Id, i.Folio, i.Method, i.Kind, i.ProjectName, i.ClientName, i.PartNumber,
            i.Location, i.InspectedOn, i.InspectorId, i.AcceptanceCriteria, i.WpsId, i.CleanlinessStandard, i.Notes, i.Status,
            SignatureDto.From(i.SignedBy), i.VoidReason, i.EquipmentIds.ToList(), i.PhotoFileIds.ToList(),
            i.WeldItems.Select(WeldItemDto.From).ToList(), i.CoatingMeasurements.Select(CoatingMeasurementDto.From).ToList(),
            i.InspectedLengthMm, i.RejectedLengthMm, i.RejectionRatePercent, i.HasRejections);
    }

    public sealed record MethodKpiDto(InspectionMethod Method, int Inspections, int WithRejections, decimal InspectedMeters, decimal RejectedMeters);

    public sealed record MonthlyTrendDto(int Year, int Month, int Inspections, decimal RejectionPercentage);

    // Dashboard de inspección (RF-9): tasas de rechazo, metros inspeccionados y tendencia.
    public sealed record InspectionKpisDto(int SignedInspections, decimal InspectedMeters, decimal RejectedMeters,
        decimal DirectAcceptancePercentage, decimal RejectionPercentage, IReadOnlyList<MethodKpiDto> ByMethod,
        IReadOnlyList<MonthlyTrendDto> Trend);
}
