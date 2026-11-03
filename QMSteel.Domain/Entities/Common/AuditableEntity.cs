namespace QMSteel.Domain.Entities.Common
{
    public abstract class AuditableEntity : Entity
    {
        public DateTime CreatedAtUtc { get; set; }
        public Guid? CreatedBy { get; set; }
        public DateTime? UpdatedAtUtc { get; set; }
        public Guid? UpdatedBy { get; set; }
    }
}
