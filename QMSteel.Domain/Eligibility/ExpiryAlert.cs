namespace QMSteel.Domain.Eligibility
{
    public sealed record ExpiryAlert(string Item, DateOnly ExpiresOn, int DaysRemaining);
}
