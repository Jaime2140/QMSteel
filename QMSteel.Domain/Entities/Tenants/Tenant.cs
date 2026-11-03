using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Standards;

namespace QMSteel.Domain.Entities.Tenants
{
    public enum BillingCycle { Monthly, Annual }

    public enum SubscriptionStatus { Trial, Active, PastDue, Suspended, Cancelled }

    // El tenant es la planta/empresa suscrita; no pertenece a otro tenant.
    public class Tenant : AuditableEntity
    {
        public string Name { get; private set; } = default!;
        public string Slug { get; private set; } = default!;
        public BillingCycle BillingCycle { get; private set; }
        public SubscriptionStatus SubscriptionStatus { get; private set; } = SubscriptionStatus.Trial;
        public DateOnly SubscriptionEndsOn { get; private set; }

        private readonly List<QualityStandard> _enabledStandards = new();
        public IReadOnlyCollection<QualityStandard> EnabledStandards => _enabledStandards;

        private Tenant() { }

        public Tenant(string name, string slug, BillingCycle billingCycle, DateOnly trialEndsOn)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new DomainException("TENANT_NAME_REQUIRED", "El nombre de la empresa es obligatorio.");
            if (string.IsNullOrWhiteSpace(slug))
                throw new DomainException("TENANT_SLUG_REQUIRED", "El identificador de la empresa es obligatorio.");

            Name = name.Trim();
            Slug = slug.Trim().ToLowerInvariant();
            BillingCycle = billingCycle;
            SubscriptionEndsOn = trialEndsOn;
            _enabledStandards.Add(QualityStandard.Aisc207);
            _enabledStandards.Add(QualityStandard.AwsD11);
        }

        public bool CanOperate(DateOnly today) =>
            SubscriptionStatus is SubscriptionStatus.Trial or SubscriptionStatus.Active or SubscriptionStatus.PastDue
            && today <= SubscriptionEndsOn;

        public void Activate(DateOnly paidThrough)
        {
            SubscriptionStatus = SubscriptionStatus.Active;
            SubscriptionEndsOn = paidThrough;
        }

        public void MarkPastDue() => SubscriptionStatus = SubscriptionStatus.PastDue;
        public void Suspend() => SubscriptionStatus = SubscriptionStatus.Suspended;
        public void Cancel() => SubscriptionStatus = SubscriptionStatus.Cancelled;

        public void EnableStandard(QualityStandard standard)
        {
            if (!_enabledStandards.Contains(standard))
                _enabledStandards.Add(standard);
        }

        public void DisableStandard(QualityStandard standard) => _enabledStandards.Remove(standard);
    }
}
