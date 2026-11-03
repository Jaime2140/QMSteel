using QMSteel.Domain.Eligibility;
using QMSteel.Domain.Entities.Equipments;
using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Entities.NonConformities;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Compliance
{
    // Fórmulas de los KPIs del tablero ejecutivo (RF-10); la carga de datos la resuelve la capa de aplicación.
    public static class QualityKpis
    {
        public static decimal Percentage(decimal part, decimal total) =>
            total == 0 ? 0 : decimal.Round(100m * part / total, 1);

        // Requisitos parcialmente cumplidos cuentan como medio punto; los no aplicables no cuentan.
        public static decimal CompliancePercentage(IEnumerable<ComplianceStatus> statuses)
        {
            var applicable = statuses.Where(s => s != ComplianceStatus.NotApplicable).ToList();
            var points = applicable.Sum(s => s switch
            {
                ComplianceStatus.Compliant => 1m,
                ComplianceStatus.PartiallyCompliant => 0.5m,
                _ => 0m
            });
            return Percentage(points, applicable.Count);
        }

        public static decimal DirectAcceptancePercentage(IEnumerable<Inspection> inspections)
        {
            var signed = inspections.Where(i => i.Status == InspectionStatus.Signed && i.IsNdt).ToList();
            var inspected = signed.Sum(i => i.InspectedLengthMm);
            return Percentage(inspected - signed.Sum(i => i.RejectedLengthMm), inspected);
        }

        public static decimal RejectionPercentage(IEnumerable<Inspection> inspections)
        {
            var signed = inspections.Where(i => i.Status == InspectionStatus.Signed && i.IsNdt).ToList();
            return Percentage(signed.Sum(i => i.RejectedLengthMm), signed.Sum(i => i.InspectedLengthMm));
        }

        public static decimal? AverageDaysToClose(IEnumerable<NonConformity> nonConformities)
        {
            var days = nonConformities.Select(n => n.DaysToClose).OfType<int>().ToList();
            return days.Count == 0 ? null : decimal.Round((decimal)days.Average(), 1);
        }

        public static decimal ClosureEffectivenessPercentage(IEnumerable<NonConformity> nonConformities)
        {
            var closed = nonConformities.Where(n => n.Status == NonConformityStatus.Closed).ToList();
            return Percentage(closed.Count(n => n.WasEffective == true), closed.Count);
        }

        public static Money CostOfPoorQuality(IEnumerable<NonConformity> nonConformities, Currency currency) =>
            nonConformities
                .Select(n => n.CostOfPoorQuality)
                .OfType<Money>()
                .Where(c => c.Currency == currency)
                .Aggregate(Money.Zero(currency), (total, cost) => total.Add(cost));

        public static decimal CalibratedEquipmentPercentage(IEnumerable<Equipment> equipments, DateOnly today)
        {
            var active = equipments.Where(e => e.IsActive).ToList();
            return Percentage(active.Count(e => e.GetStatus(today) is ResourceStatus.Valid or ResourceStatus.ExpiringSoon), active.Count);
        }
    }
}
