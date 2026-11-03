using QMSteel.Domain.Execution;

namespace QMSteel.Domain.ValueObjects
{
    public enum Currency { USD, MXN }

    public sealed record Money
    {
        public decimal Amount { get; private init; }
        public Currency Currency { get; private init; }

        private Money() { }

        public Money(decimal amount, Currency currency)
        {
            if (amount < 0)
                throw new DomainException("MONEY_NEGATIVE", "El monto no puede ser negativo.");

            Amount = decimal.Round(amount, 2, MidpointRounding.AwayFromZero);
            Currency = currency;
        }

        public static Money Zero(Currency currency) => new(0, currency);

        public Money Add(Money other)
        {
            if (other.Currency != Currency)
                throw new DomainException("MONEY_CURRENCY_MISMATCH", "No se pueden sumar montos de monedas distintas.");

            return new(Amount + other.Amount, Currency);
        }
    }
}
