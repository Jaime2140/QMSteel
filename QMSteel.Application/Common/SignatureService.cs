using QMSteel.Application.Abstractions;
using QMSteel.Domain.Entities.Users;
using QMSteel.Domain.Execution;
using QMSteel.Domain.Repositories;
using QMSteel.Domain.ValueObjects;

namespace QMSteel.Application.Common
{
    public interface ISignatureService
    {
        // Firma electrónica del usuario en sesión. La API es responsable de pedir la reautenticación antes de llamar aquí.
        Task<Result<ElectronicSignature>> SignAsync(string meaning, bool requireApprover = false, CancellationToken ct = default);
    }

    internal class SignatureService : ISignatureService
    {
        private readonly ICurrentUserContext _currentUser;
        private readonly IUserRepository _users;
        private readonly TimeProvider _time;

        public SignatureService(ICurrentUserContext currentUser, IUserRepository users, TimeProvider time)
        {
            _currentUser = currentUser;
            _users = users;
            _time = time;
        }

        public async Task<Result<ElectronicSignature>> SignAsync(string meaning, bool requireApprover = false, CancellationToken ct = default)
        {
            var user = _currentUser.UserId is { } userId ? await _users.GetByIdAsync(userId, ct) : null;
            if (user is null)
                return Result.Failure<ElectronicSignature>(ApplicationErrors.UserNotRegistered);

            var denied = Check(user, requireApprover);
            if (denied is not null)
                return Result.Failure<ElectronicSignature>(denied);

            return new ElectronicSignature(user.Id, user.FullName, meaning, _time.UtcNow());
        }

        private static Error? Check(User user, bool requireApprover)
        {
            if (!user.IsActive) return ApplicationErrors.UserInactive;
            if (user.IsReadOnly) return ApplicationErrors.ReadOnlyUser;
            if (requireApprover && !user.CanApprove) return ApplicationErrors.ApproverRequired;
            return null;
        }
    }
}
