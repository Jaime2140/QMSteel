using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;

namespace QMSteel.Domain.Entities.Compliance
{
    public enum TargetDirection { HigherIsBetter, LowerIsBetter }

    public class ObjectiveMeasurement : Entity
    {
        public int Year { get; private set; }
        public int Month { get; private set; }
        public decimal Value { get; private set; }
        public string? Comment { get; private set; }

        private ObjectiveMeasurement() { }

        internal ObjectiveMeasurement(int year, int month, decimal value, string? comment)
        {
            Year = year;
            Month = month;
            Value = value;
            Comment = comment;
        }

        internal void Update(decimal value, string? comment)
        {
            Value = value;
            Comment = comment;
        }
    }

    // Objetivo de calidad con meta y mediciones mensuales para el tablero ejecutivo.
    public class QualityObjective : AggregateRoot
    {
        public string Name { get; private set; } = default!;
        public string Indicator { get; private set; } = default!;
        public string Unit { get; private set; } = default!;
        public decimal Target { get; private set; }
        public TargetDirection Direction { get; private set; }
        public int Year { get; private set; }
        public string? ResponsibleName { get; private set; }

        private readonly List<ObjectiveMeasurement> _measurements = new();
        public IReadOnlyCollection<ObjectiveMeasurement> Measurements => _measurements;

        public ObjectiveMeasurement? LatestMeasurement =>
            _measurements.OrderByDescending(m => m.Year).ThenByDescending(m => m.Month).FirstOrDefault();

        private QualityObjective() { }

        public QualityObjective(Guid tenantId, string name, string indicator, string unit, decimal target, TargetDirection direction,
            int year, string? responsibleName = null)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("OBJECTIVE_NAME_REQUIRED", "El nombre del objetivo es obligatorio.");
            if (string.IsNullOrWhiteSpace(indicator))
                throw new DomainException("OBJECTIVE_INDICATOR_REQUIRED", "El indicador es obligatorio.");
            if (string.IsNullOrWhiteSpace(unit))
                throw new DomainException("OBJECTIVE_UNIT_REQUIRED", "La unidad es obligatoria.");

            TenantId = tenantId;
            Name = name.Trim();
            Indicator = indicator.Trim();
            Unit = unit.Trim();
            Target = target;
            Direction = direction;
            Year = year;
            ResponsibleName = responsibleName?.Trim();
        }

        public void ChangeTarget(decimal target) => Target = target;

        public void RecordMeasurement(int month, decimal value, string? comment = null)
        {
            if (month is < 1 or > 12)
                throw new DomainException("OBJECTIVE_MONTH_INVALID", "El mes debe estar entre 1 y 12.");

            var existing = _measurements.SingleOrDefault(m => m.Year == Year && m.Month == month);
            if (existing is not null)
                existing.Update(value, comment?.Trim());
            else
                _measurements.Add(new ObjectiveMeasurement(Year, month, value, comment?.Trim()));
        }

        public bool IsMet(decimal value) => Direction == TargetDirection.HigherIsBetter ? value >= Target : value <= Target;

        public bool? IsCurrentlyMet => LatestMeasurement is { } latest ? IsMet(latest.Value) : null;
    }
}
