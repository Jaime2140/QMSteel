using QMSteel.Domain.Entities.Common;

namespace QMSteel.Domain.Entities.Documents
{
    public sealed record DocumentRevisionApproved(Guid DocumentId, Guid RevisionId, int RevisionNumber, DateTime OccurredAtUtc) : IDomainEvent;

    public sealed record ControlledCopiesRevoked(Guid DocumentId, IReadOnlyList<Guid> CopyIds, DateTime OccurredAtUtc) : IDomainEvent;
}
