using QMSteel.Domain.Entities.Users;

namespace QMSteel.Domain.Repositories;

public interface IUserRepository : IRepository<User>
{
    Task<User?> GetByIdentityIdAsync(string identityId, CancellationToken ct = default);
    Task<User?> GetByEmailAsync(string email, CancellationToken ct = default);
    Task<IReadOnlyList<User>> GetAllAsync(bool? onlyActive = null, CancellationToken ct = default);
}
