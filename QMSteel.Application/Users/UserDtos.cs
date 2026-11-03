using QMSteel.Domain.Entities.Tenants;
using QMSteel.Domain.Entities.Users;
using QMSteel.Domain.Standards;

namespace QMSteel.Application.Users
{
    // IdentityId es el identificador del usuario en el proveedor de identidad (claim "sub" del token).
    public sealed record CreateUserRequest(string IdentityId, string Email, string FullName, UserRole Role, string? Position = null,
        string? Area = null);

    public sealed record UpdateUserRequest(string FullName, string? Position, string? Area);

    public sealed record UserDto(Guid Id, string IdentityId, string Email, string FullName, string? Position, string? Area, UserRole Role,
        bool IsActive, bool CanApprove, bool IsReadOnly)
    {
        public static UserDto From(User u) => new(u.Id, u.IdentityId, u.Email, u.FullName, u.Position, u.Area, u.Role, u.IsActive,
            u.CanApprove, u.IsReadOnly);
    }

    public sealed record TenantDto(Guid Id, string Name, string Slug, BillingCycle BillingCycle, SubscriptionStatus SubscriptionStatus,
        DateOnly SubscriptionEndsOn, bool CanOperate, IReadOnlyList<QualityStandard> EnabledStandards)
    {
        public static TenantDto From(Tenant t, DateOnly today) => new(t.Id, t.Name, t.Slug, t.BillingCycle, t.SubscriptionStatus,
            t.SubscriptionEndsOn, t.CanOperate(today), t.EnabledStandards.ToList());
    }
}
