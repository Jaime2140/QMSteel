using QMSteel.Domain.Entities.Common;

namespace QMSteel.Domain.Entities.NonConformities
{
    public sealed record NonConformityOpened(Guid NonConformityId, string Folio, Criticality Criticality, DateTime OccurredAtUtc) : IDomainEvent;

    public sealed record NonConformityCommitteeAssigned(Guid NonConformityId, IReadOnlyList<Guid> MemberUserIds, DateTime OccurredAtUtc) : IDomainEvent;

    public sealed record NonConformityClosed(Guid NonConformityId, string Folio, bool WasEffective, DateTime OccurredAtUtc) : IDomainEvent;
}
