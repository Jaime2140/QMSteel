using QMSteel.Domain.Entities.Compliance;
using QMSteel.Domain.Standards;

namespace QMSteel.Application.Compliance
{
    public sealed record AssessRequirementRequest(ComplianceStatus Status, string? Notes = null, DateOnly? AssessedOn = null);

    public sealed record ComplianceRowDto(Guid RequirementId, string Edition, string Clause, string Title, EvidenceSource EvidenceSource,
        Guid? AssessmentId, ComplianceStatus? Status, DateOnly? AssessedOn, bool IsAutomatic, string? Notes, IReadOnlyList<Guid> EvidenceFileIds);

    // Matriz de rastreo normativo: el porcentaje considera solo requisitos evaluados; los pendientes se reportan aparte.
    public sealed record ComplianceMatrixDto(QualityStandard Standard, string? ClausePrefix, decimal CompliancePercentage,
        int Requirements, int Pending, IReadOnlyList<ComplianceRowDto> Rows);

    public sealed record CreateObjectiveRequest(string Name, string Indicator, string Unit, decimal Target, TargetDirection Direction,
        int Year, string? ResponsibleName = null);

    public sealed record RecordMeasurementRequest(int Month, decimal Value, string? Comment = null);

    public sealed record MeasurementDto(Guid Id, int Year, int Month, decimal Value, string? Comment, bool IsMet);

    public sealed record QualityObjectiveDto(Guid Id, string Name, string Indicator, string Unit, decimal Target, TargetDirection Direction,
        int Year, string? ResponsibleName, decimal? LatestValue, bool? IsCurrentlyMet, IReadOnlyList<MeasurementDto> Measurements)
    {
        public static QualityObjectiveDto From(QualityObjective o) => new(o.Id, o.Name, o.Indicator, o.Unit, o.Target, o.Direction, o.Year,
            o.ResponsibleName, o.LatestMeasurement?.Value, o.IsCurrentlyMet,
            o.Measurements.OrderBy(m => m.Year).ThenBy(m => m.Month)
                .Select(m => new MeasurementDto(m.Id, m.Year, m.Month, m.Value, m.Comment, o.IsMet(m.Value))).ToList());
    }
}
