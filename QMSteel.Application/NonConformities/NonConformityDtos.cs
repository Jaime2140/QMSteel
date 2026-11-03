using QMSteel.Application.Common;
using QMSteel.Domain.Entities.NonConformities;

namespace QMSteel.Application.NonConformities
{
    // Paso 1 del formulario: detalles iniciales.
    public sealed record OpenNonConformityRequest(NonConformitySource Source, Criticality Criticality, DefectType DefectType,
        string Description, DateOnly? DetectedOn = null, Guid? SourceReferenceId = null, Guid? SupplierId = null,
        string? ProjectName = null, string? ClientName = null, string? PartNumber = null, IReadOnlyList<Guid>? EvidenceFileIds = null);

    public sealed record UpdateNonConformityRequest(Criticality Criticality, DefectType DefectType, string Description,
        string? ProjectName, string? ClientName, string? PartNumber);

    // Paso 2: análisis de causa raíz con 5 porqués.
    public sealed record RootCauseRequest(IReadOnlyList<string> Whys, string RootCause, bool RequiresCorrectiveAction);

    // Paso 3: acciones.
    public sealed record AddActionRequest(ActionType Type, string Description, string ResponsibleName, DateOnly DueOn,
        Guid? ResponsibleUserId = null);

    public sealed record CompleteActionRequest(string? Notes = null, Guid? EvidenceFileId = null, DateOnly? CompletedOn = null);

    public sealed record CloseNonConformityRequest(bool WasEffective, DateOnly? ClosedOn = null);

    public sealed record ActionDto(Guid Id, ActionType Type, string Description, string ResponsibleName, Guid? ResponsibleUserId,
        DateOnly DueOn, DateOnly? CompletedOn, string? CompletionNotes, Guid? EvidenceFileId, bool IsOverdue)
    {
        public static ActionDto From(CorrectiveAction a, DateOnly today) => new(a.Id, a.Type, a.Description, a.ResponsibleName,
            a.ResponsibleUserId, a.DueOn, a.CompletedOn, a.CompletionNotes, a.EvidenceFileId, a.IsOverdueOn(today));
    }

    // Renglón de la tabla de desviaciones activas.
    public sealed record NonConformitySummaryDto(Guid Id, string Folio, Criticality Criticality, NonConformitySource Source,
        DefectType DefectType, string? ProjectName, string? ClientName, string? PartNumber, DateOnly DetectedOn,
        NonConformityStatus Status, bool IsOverdue)
    {
        public static NonConformitySummaryDto From(NonConformity n, DateOnly today) => new(n.Id, n.Folio, n.Criticality, n.Source,
            n.DefectType, n.ProjectName, n.ClientName, n.PartNumber, n.DetectedOn, n.Status, n.IsOverdueOn(today));
    }

    public sealed record NonConformityDto(Guid Id, string Folio, NonConformitySource Source, Guid? SourceReferenceId, Guid? SupplierId,
        Criticality Criticality, DefectType DefectType, string Description, string? ProjectName, string? ClientName, string? PartNumber,
        DateOnly DetectedOn, Guid ReportedByUserId, NonConformityStatus Status, Disposition? Disposition,
        IReadOnlyList<Guid> EvidenceFileIds, IReadOnlyList<string> Whys, string? RootCause, bool RequiresCorrectiveAction,
        IReadOnlyList<Guid> CommitteeUserIds, IReadOnlyList<ActionDto> Actions, MoneyDto? CostOfPoorQuality, DateOnly? ClosedOn,
        bool? WasEffective, SignatureDto? ClosedBy, string? CancellationReason, int? DaysToClose, bool IsOverdue)
    {
        public static NonConformityDto From(NonConformity n, DateOnly today) => new(n.Id, n.Folio, n.Source, n.SourceReferenceId,
            n.SupplierId, n.Criticality, n.DefectType, n.Description, n.ProjectName, n.ClientName, n.PartNumber, n.DetectedOn,
            n.ReportedByUserId, n.Status, n.Disposition, n.EvidenceFileIds.ToList(), n.Whys.ToList(), n.RootCause,
            n.RequiresCorrectiveAction, n.CommitteeUserIds.ToList(), n.Actions.OrderBy(a => a.DueOn).Select(a => ActionDto.From(a, today)).ToList(),
            MoneyDto.From(n.CostOfPoorQuality), n.ClosedOn, n.WasEffective, SignatureDto.From(n.ClosedBy), n.CancellationReason,
            n.DaysToClose, n.IsOverdueOn(today));
    }

    // Monitor de NCs del módulo RF-7.
    public sealed record NonConformityMonitorDto(int Open, int Overdue, decimal ClosureEffectivenessPercentage,
        decimal? AverageDaysToClose, MoneyDto CostOfPoorQuality);

    internal static class NonConformityFolio
    {
        public const string Prefix = "NC";
    }
}
