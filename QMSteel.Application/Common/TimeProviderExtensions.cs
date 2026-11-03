namespace QMSteel.Application.Common
{
    internal static class TimeProviderExtensions
    {
        // Fecha de planta: los vencimientos se evalúan con la fecha local, no con la UTC.
        public static DateOnly Today(this TimeProvider time) => DateOnly.FromDateTime(time.GetLocalNow().DateTime);

        public static DateTime UtcNow(this TimeProvider time) => time.GetUtcNow().UtcDateTime;
    }
}
