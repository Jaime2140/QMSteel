using QMSteel.Domain.Entities.Common;

namespace QMSteel.Domain.Entities.Receiving
{
    public enum MaterialType { Steel, WeldingConsumable, Coating, Fastener, Other }

    public enum MtrStatus { NotRequired, Complete, Incomplete, Missing }

    public enum VisualResult { Pending, Accepted, Rejected }

    public enum ReceivingDefect { Oxidation, PhysicalDamage, DimensionalDeviation, MissingIdentification, WrongMaterial, QuantityMismatch }

    public class ReceivedItem : Entity
    {
        public MaterialType MaterialType { get; private set; }
        public string Description { get; private set; } = default!;
        public string? Specification { get; private set; }
        public string? HeatNumber { get; private set; }
        public decimal Quantity { get; private set; }
        public string Unit { get; private set; } = default!;
        public MtrStatus MtrStatus { get; private set; }
        public Guid? MtrFileId { get; private set; }
        public VisualResult VisualResult { get; private set; } = VisualResult.Pending;
        public string? Notes { get; private set; }

        private readonly List<ReceivingDefect> _defects = new();
        public IReadOnlyCollection<ReceivingDefect> Defects => _defects;

        public bool IsConforming => VisualResult == VisualResult.Accepted
            && MtrStatus is MtrStatus.NotRequired or MtrStatus.Complete;

        private ReceivedItem() { }

        internal ReceivedItem(MaterialType materialType, string description, string? specification, string? heatNumber,
            decimal quantity, string unit, MtrStatus mtrStatus, Guid? mtrFileId)
        {
            MaterialType = materialType;
            Description = description;
            Specification = specification;
            HeatNumber = heatNumber;
            Quantity = quantity;
            Unit = unit;
            MtrStatus = mtrStatus;
            MtrFileId = mtrFileId;
        }

        internal void UpdateMtr(MtrStatus status, Guid? fileId)
        {
            MtrStatus = status;
            MtrFileId = fileId;
        }

        internal void RecordVisual(bool accepted, IEnumerable<ReceivingDefect> defects, string? notes)
        {
            VisualResult = accepted ? VisualResult.Accepted : VisualResult.Rejected;
            _defects.Clear();
            _defects.AddRange(defects.Distinct());
            Notes = notes;
        }

        public string DescribeDeviation()
        {
            var parts = new List<string>();
            if (MtrStatus is MtrStatus.Incomplete or MtrStatus.Missing)
                parts.Add(MtrStatus == MtrStatus.Missing ? "sin MTR" : "MTR incompleto");
            if (_defects.Count > 0)
                parts.Add(string.Join(", ", _defects));
            if (!string.IsNullOrWhiteSpace(Notes))
                parts.Add(Notes);
            var heat = HeatNumber is null ? string.Empty : $" (colada {HeatNumber})";
            return $"{Description}{heat}: {string.Join("; ", parts)}";
        }
    }
}
