using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Welding;

namespace QMSteel.Domain.Entities.Inspections
{
    public sealed record WeldActivity(Guid WelderId, WeldingProcess Process);

    // Al firmarse alimenta la continuidad de soldadores (RF-3) y la frecuencia de uso de equipos (RF-2).
    public sealed record InspectionSigned(Guid InspectionId, DateOnly InspectedOn, IReadOnlyList<WeldActivity> WeldActivities,
        IReadOnlyList<Guid> EquipmentIds, DateTime OccurredAtUtc) : IDomainEvent;

    public sealed record InspectionRejectionsFound(Guid InspectionId, string Folio, InspectionMethod Method,
        IReadOnlyList<string> RejectedItems, DateTime OccurredAtUtc) : IDomainEvent;
}
