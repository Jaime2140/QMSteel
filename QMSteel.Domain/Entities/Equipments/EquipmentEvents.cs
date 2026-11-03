using QMSteel.Domain.Entities.Common;

namespace QMSteel.Domain.Entities.Equipments
{
    public sealed record EquipmentVerificationFailed(Guid EquipmentId, string Code, DateOnly PerformedOn, DateTime OccurredAtUtc) : IDomainEvent;

    public sealed record EquipmentCalibrationFailed(Guid EquipmentId, string Code, DateOnly CalibratedOn, DateTime OccurredAtUtc) : IDomainEvent;
}
