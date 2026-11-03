using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Domain.Entities.Equipments
{
    // Verificación diaria o periódica realizada en planta.
    public class EquipmentVerification : Entity
    {
        public DateOnly PerformedOn { get; private set; }
        public bool Passed { get; private set; }
        public string? Notes { get; private set; }
        public ElectronicSignature PerformedBy { get; private set; } = default!;

        private EquipmentVerification() { }

        internal EquipmentVerification(DateOnly performedOn, bool passed, string? notes, ElectronicSignature performedBy)
        {
            PerformedOn = performedOn;
            Passed = passed;
            Notes = notes;
            PerformedBy = performedBy;
        }
    }
}
