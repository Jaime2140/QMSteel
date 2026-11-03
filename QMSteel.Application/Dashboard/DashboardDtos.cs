using QMSteel.Application.Common;
using QMSteel.Application.Compliance;
using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Standards;

namespace QMSteel.Application.Dashboard
{
    public enum AlertModule { Documents, Equipments, Welders, Inspectors }

    // Renglón del tablero de alertas de vencimiento (días negativos = ya vencido).
    public sealed record ExpiryAlertDto(AlertModule Module, Guid ResourceId, string ResourceName, string Item, DateOnly ExpiresOn,
        int DaysRemaining, ResourceStatus Status);

    // AuditCompliancePercentage es null si en el periodo no hubo auditorías completadas de esa norma.
    public sealed record StandardComplianceDto(QualityStandard Standard, decimal? AuditCompliancePercentage, decimal MatrixCompliancePercentage);

    // Dashboard ejecutivo de RF-10.
    public sealed record ExecutiveDashboardDto(DateOnly From, DateOnly To, IReadOnlyList<StandardComplianceDto> Compliance,
        decimal DirectAcceptancePercentage, decimal RejectionPercentage, int OpenNonConformities, decimal? AverageDaysToCloseNonConformities,
        decimal ClosureEffectivenessPercentage, MoneyDto CostOfPoorQualityUsd, MoneyDto CostOfPoorQualityMxn,
        decimal CalibratedEquipmentPercentage, int CalibratedEquipment, int ExpiredOrBlockedEquipment,
        IReadOnlyList<QualityObjectiveDto> Objectives);
}
