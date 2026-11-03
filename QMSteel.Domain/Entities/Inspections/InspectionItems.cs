using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Welding;

namespace QMSteel.Domain.Entities.Inspections
{
    public enum InspectionKind { Weld, Coating }

    public enum InspectionMethod { VT, PT, MT, UT, RT, SurfacePreparation, WFT, DFT }

    public enum InspectionStatus { Draft, Signed, Voided }

    public enum ItemResult { Accepted, Rejected }

    public class WeldInspectionItem : Entity
    {
        public string JointNumber { get; private set; } = default!;
        public Guid WelderId { get; private set; }
        public string WelderStamp { get; private set; } = default!;
        public WeldingProcess Process { get; private set; }
        public string? Position { get; private set; }
        public bool IsTackWeld { get; private set; }
        public decimal InspectedLengthMm { get; private set; }
        public decimal RejectedLengthMm { get; private set; }
        public string? Discontinuities { get; private set; }

        public ItemResult Result => RejectedLengthMm > 0 ? ItemResult.Rejected : ItemResult.Accepted;

        private WeldInspectionItem() { }

        internal WeldInspectionItem(string jointNumber, Guid welderId, string welderStamp, WeldingProcess process, string? position,
            bool isTackWeld, decimal inspectedLengthMm, decimal rejectedLengthMm, string? discontinuities)
        {
            JointNumber = jointNumber;
            WelderId = welderId;
            WelderStamp = welderStamp;
            Process = process;
            Position = position;
            IsTackWeld = isTackWeld;
            InspectedLengthMm = inspectedLengthMm;
            RejectedLengthMm = rejectedLengthMm;
            Discontinuities = discontinuities;
        }
    }

    // Lecturas de espesor (WFT/DFT) o perfil de anclaje (preparación de superficie), en micras.
    public class CoatingMeasurement : Entity
    {
        // Una lectura puntual puede quedar hasta 20% por debajo del mínimo si el promedio cumple (criterio tipo SSPC-PA 2).
        public const decimal MinimumReadingFactor = 0.8m;

        public string Location { get; private set; } = default!;
        public decimal SpecifiedMinMicrons { get; private set; }
        public decimal SpecifiedMaxMicrons { get; private set; }

        private readonly List<decimal> _readings = new();
        public IReadOnlyCollection<decimal> Readings => _readings;

        public decimal Average => _readings.Count == 0 ? 0 : decimal.Round(_readings.Average(), 1);
        public decimal Lowest => _readings.Count == 0 ? 0 : _readings.Min();
        public decimal Highest => _readings.Count == 0 ? 0 : _readings.Max();

        public ItemResult Result =>
            Average >= SpecifiedMinMicrons && Average <= SpecifiedMaxMicrons && Lowest >= SpecifiedMinMicrons * MinimumReadingFactor
                ? ItemResult.Accepted
                : ItemResult.Rejected;

        private CoatingMeasurement() { }

        internal CoatingMeasurement(string location, decimal specifiedMinMicrons, decimal specifiedMaxMicrons, IEnumerable<decimal> readings)
        {
            Location = location;
            SpecifiedMinMicrons = specifiedMinMicrons;
            SpecifiedMaxMicrons = specifiedMaxMicrons;
            _readings.AddRange(readings);
        }
    }
}
