namespace QMSteel.Infrastructure.Persistence
{
    // Consecutivo de folios por tenant, prefijo y año.
    public class FolioSequence
    {
        public Guid TenantId { get; set; }
        public string Prefix { get; set; } = default!;
        public int Year { get; set; }
        public int LastNumber { get; set; }
    }
}
