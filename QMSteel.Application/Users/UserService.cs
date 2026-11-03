using QMSteel.Application.Common;
using QMSteel.Domain.Entities.Users;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Users
{
    // Perfiles de negocio de los usuarios de la planta; las credenciales las administra el proveedor de identidad.
    public sealed class UserService : ApplicationService
    {
        private readonly IUserRepository _users;

        public UserService(UseCaseContext context, IUserRepository users) : base(context)
        {
            _users = users;
        }

        public async Task<IReadOnlyList<UserDto>> ListAsync(bool? onlyActive = null, CancellationToken ct = default) =>
            (await _users.GetAllAsync(onlyActive, ct)).Select(UserDto.From).ToList();

        public Task<Result<UserDto>> GetAsync(Guid id, CancellationToken ct = default) =>
            GetAsync(_users, id, UserDto.From, ct);

        public Task<Result<UserDto>> GetCurrentAsync(CancellationToken ct = default) =>
            GetAsync(_users, UserId, UserDto.From, ct);

        public async Task<Result<UserDto>> CreateAsync(CreateUserRequest request, CancellationToken ct = default)
        {
            if (await _users.GetByIdentityIdAsync(request.IdentityId.Trim(), ct) is not null)
                return Result.Failure<UserDto>(ApplicationErrors.Duplicate("USER_IDENTITY_DUPLICATE", "La identidad ya está ligada a otro usuario."));
            if (await _users.GetByEmailAsync(request.Email, ct) is not null)
                return Result.Failure<UserDto>(ApplicationErrors.Duplicate("USER_EMAIL_DUPLICATE", $"Ya existe un usuario con el correo {request.Email}."));

            var user = new User(TenantId, request.IdentityId, request.Email, request.FullName, request.Role, request.Position, request.Area);
            return await AddAsync(_users, user, UserDto.From, ct);
        }

        public Task<Result<UserDto>> UpdateAsync(Guid id, UpdateUserRequest request, CancellationToken ct = default) =>
            ChangeAsync(_users, id, u => u.UpdateProfile(request.FullName, request.Position, request.Area), UserDto.From, ct);

        // Un administrador no puede quitarse a sí mismo el rol ni desactivarse: la planta podría quedar sin administrador.
        public Task<Result<UserDto>> ChangeRoleAsync(Guid id, UserRole role, CancellationToken ct = default) =>
            id == UserId
                ? Task.FromResult(Result.Failure<UserDto>(SelfChange))
                : ChangeAsync(_users, id, u => u.ChangeRole(role), UserDto.From, ct);

        public Task<Result<UserDto>> DeactivateAsync(Guid id, CancellationToken ct = default) =>
            id == UserId
                ? Task.FromResult(Result.Failure<UserDto>(SelfChange))
                : ChangeAsync(_users, id, u => u.Deactivate(), UserDto.From, ct);

        public Task<Result<UserDto>> ActivateAsync(Guid id, CancellationToken ct = default) =>
            ChangeAsync(_users, id, u => u.Activate(), UserDto.From, ct);

        private static readonly Error SelfChange =
            Error.Validation("USER_SELF_CHANGE", "No puedes cambiar tu propio rol ni desactivar tu propio usuario.");
    }
}
