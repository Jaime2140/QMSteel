using QMSteel.Domain.Eligibility;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Application.Common
{
    public sealed record SignatureDto(Guid UserId, string SignerName, string Meaning, DateTime SignedAtUtc)
    {
        public static SignatureDto? From(ElectronicSignature? signature) =>
            signature is null ? null : new(signature.UserId, signature.SignerName, signature.Meaning, signature.SignedAtUtc);
    }

    public sealed record MoneyDto(decimal Amount, Currency Currency)
    {
        public static MoneyDto? From(Money? money) => money is null ? null : new(money.Amount, money.Currency);

        public Money ToDomain() => new(Amount, Currency);
    }

    public sealed record RangeDto(decimal Min, decimal Max)
    {
        public static RangeDto From(NumericRange range) => new(range.Min, range.Max);

        public NumericRange ToDomain() => new(Min, Max);
    }

    // Motivo de bloqueo para el modal rojo (RNF 3.2.5) cuando un recurso no puede usarse.
    public sealed record EligibilityDto(bool IsEligible, string? ReasonCode, string? Reason)
    {
        public static EligibilityDto From(EligibilityResult result) => new(result.IsEligible, result.ReasonCode, result.Reason);
    }
}
