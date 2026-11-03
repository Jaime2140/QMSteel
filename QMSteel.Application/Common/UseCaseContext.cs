using QMSteel.Application.Abstractions;
using QMSteel.Domain.Repositories;

namespace QMSteel.Application.Common
{
    // Dependencias que comparten todos los casos de uso; evita repetirlas en cada constructor.
    public sealed class UseCaseContext
    {
        public UseCaseContext(ICurrentUserContext currentUser, IUnitOfWork unitOfWork, TimeProvider time, ISignatureService signatures)
        {
            CurrentUser = currentUser;
            UnitOfWork = unitOfWork;
            Time = time;
            Signatures = signatures;
        }

        public ICurrentUserContext CurrentUser { get; }
        public IUnitOfWork UnitOfWork { get; }
        public TimeProvider Time { get; }
        public ISignatureService Signatures { get; }
    }
}
