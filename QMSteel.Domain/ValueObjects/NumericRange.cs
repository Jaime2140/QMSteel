using QMSteel.Domain.Execution;

namespace QMSteel.Domain.ValueObjects
{
    public sealed record NumericRange
    {
        public decimal Min { get; private init; }
        public decimal Max { get; private init; }

        private NumericRange() { }

        public NumericRange(decimal min, decimal max)
        {
            if (min > max)
                throw new DomainException("RANGE_INVALID", "El mínimo no puede ser mayor que el máximo.");

            Min = min;
            Max = max;
        }

        public static NumericRange Exactly(decimal value) => new(value, value);

        public bool Contains(decimal value) => value >= Min && value <= Max;

        public bool Contains(NumericRange other) => other.Min >= Min && other.Max <= Max;

        public override string ToString() => Min == Max ? $"{Min}" : $"{Min} - {Max}";
    }
}
