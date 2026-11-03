namespace QMSteel.Domain.Eligibility
{
    // Umbrales de aviso previos a un vencimiento (RF-5: 60, 30 y 15 días).
    public static class ExpiryAlerts
    {
        public static readonly IReadOnlyList<int> DefaultThresholds = new[] { 60, 30, 15 };

        public static int DaysUntil(DateOnly expiresOn, DateOnly today) => expiresOn.DayNumber - today.DayNumber;

        // Regresa el umbral que se cumple exactamente hoy, para disparar un solo aviso por umbral.
        public static int? ThresholdReachedOn(DateOnly expiresOn, DateOnly today, IEnumerable<int>? thresholds = null)
        {
            var days = DaysUntil(expiresOn, today);
            return (thresholds ?? DefaultThresholds).Contains(days) ? days : null;
        }

        public static ResourceStatus StatusFor(DateOnly expiresOn, DateOnly today, int expiringSoonDays)
        {
            var days = DaysUntil(expiresOn, today);
            if (days < 0) return ResourceStatus.Expired;
            return days <= expiringSoonDays ? ResourceStatus.ExpiringSoon : ResourceStatus.Valid;
        }
    }
}
