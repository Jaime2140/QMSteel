using QMSteel.Domain.Entities.Common;
using QMSteel.Domain.Execution;

namespace QMSteel.Domain.Entities.Users
{
    public enum UserRole { Administrator, QualityManager, QcInspector, AreaUser, WarehouseReceiver, Auditor }

    // Perfil de negocio del usuario; las credenciales viven en el proveedor de identidad.
    public class User : AggregateRoot
    {
        public string IdentityId { get; private set; } = default!;
        public string Email { get; private set; } = default!;
        public string FullName { get; private set; } = default!;
        public string? Position { get; private set; }
        public string? Area { get; private set; }
        public UserRole Role { get; private set; }
        public bool IsActive { get; private set; } = true;

        private User() { }

        public User(Guid tenantId, string identityId, string email, string fullName, UserRole role, string? position = null, string? area = null)
        {
            if (string.IsNullOrWhiteSpace(identityId))
                throw new DomainException("USER_IDENTITY_REQUIRED", "El usuario requiere una identidad.");
            if (string.IsNullOrWhiteSpace(email) || !email.Contains('@'))
                throw new DomainException("USER_EMAIL_INVALID", "El correo del usuario no es válido.");
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("USER_NAME_REQUIRED", "El nombre del usuario es obligatorio.");

            TenantId = tenantId;
            IdentityId = identityId;
            Email = email.Trim().ToLowerInvariant();
            FullName = fullName.Trim();
            Role = role;
            Position = position?.Trim();
            Area = area?.Trim();
        }

        public bool IsReadOnly => Role == UserRole.Auditor;

        public bool CanApprove => Role is UserRole.Administrator or UserRole.QualityManager;

        public void ChangeRole(UserRole role) => Role = role;

        public void UpdateProfile(string fullName, string? position, string? area)
        {
            if (string.IsNullOrWhiteSpace(fullName))
                throw new DomainException("USER_NAME_REQUIRED", "El nombre del usuario es obligatorio.");

            FullName = fullName.Trim();
            Position = position?.Trim();
            Area = area?.Trim();
        }

        public void Deactivate() => IsActive = false;
        public void Activate() => IsActive = true;
    }
}
