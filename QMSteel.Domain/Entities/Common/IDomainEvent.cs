namespace QMSteel.Domain.Entities.Common
{
    public interface IDomainEvent
    {
        DateTime OccurredAtUtc { get; }
    }
}
