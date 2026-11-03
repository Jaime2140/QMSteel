namespace QMSteel.Domain.Entities.Common
{
    public abstract class TenantEntity : AuditableEntity, ITenantEntity
    {
        public Guid TenantId { get; set; }
    }
}
