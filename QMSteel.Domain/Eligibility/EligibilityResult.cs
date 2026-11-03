namespace QMSteel.Domain.Eligibility
{
    public record EligibilityResult(bool IsEligible, string? ReasonCode, string? Reason)
    {
        public static EligibilityResult Ok() => new(true, null, null);
        public static EligibilityResult Blocked(string code, string reason) => new(false, code, reason);
    }
}
