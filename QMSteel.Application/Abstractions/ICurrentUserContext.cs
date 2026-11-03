namespace QMSteel.Application.Abstractions
{
    // Usuario y tenant de la petición en curso; lo implementa la API a partir del token.
    public interface ICurrentUserContext
    {
        Guid? UserId { get; }
        Guid? TenantId { get; }
    }
}
