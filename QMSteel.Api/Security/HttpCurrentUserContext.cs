using QMSteel.Application.Abstractions;
using QMSteel.Domain.Entities.Users;

namespace QMSteel.Api.Security
{
    // Usuario de la petición en curso; lo llena CurrentUserMiddleware a partir del token.
    public sealed class HttpCurrentUserContext : ICurrentUserContext
    {
        public Guid? UserId { get; private set; }
        public Guid? TenantId { get; private set; }
        public UserRole? Role { get; private set; }

        public void SignIn(User user)
        {
            UserId = user.Id;
            TenantId = user.TenantId;
            Role = user.Role;
        }
    }
}
