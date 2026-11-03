using QMSteel.Application.Abstractions;
using QMSteel.Domain.Entities.Inspections;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Inspections
{
    // La firma de la inspección es la evidencia de continuidad del soldador (RF-3) y del uso del equipo (RF-2).
    internal sealed class InspectionSignedHandler : IDomainEventHandler<InspectionSigned>
    {
        private readonly IWelderRepository _welders;
        private readonly IEquipmentRepository _equipments;

        public InspectionSignedHandler(IWelderRepository welders, IEquipmentRepository equipments)
        {
            _welders = welders;
            _equipments = equipments;
        }

        public async Task HandleAsync(InspectionSigned domainEvent, CancellationToken ct = default)
        {
            foreach (var activities in domainEvent.WeldActivities.GroupBy(a => a.WelderId))
            {
                var welder = await _welders.GetByIdAsync(activities.Key, ct);
                if (welder is null)
                    continue;

                foreach (var activity in activities)
                    welder.RegisterWeldingActivity(activity.Process, domainEvent.InspectedOn);
                _welders.Update(welder);
            }

            foreach (var equipment in await _equipments.GetByIdsAsync(domainEvent.EquipmentIds, ct))
            {
                equipment.RegisterUse(domainEvent.InspectedOn);
                _equipments.Update(equipment);
            }
        }
    }
}
