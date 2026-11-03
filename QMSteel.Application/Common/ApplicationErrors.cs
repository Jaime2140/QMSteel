using QMSteel.Domain.Execution;

namespace QMSteel.Application.Common
{
    // Errores propios de los casos de uso; las reglas de negocio llegan como DomainException desde el dominio.
    public static class ApplicationErrors
    {
        public static Error NotFound<T>(Guid id) =>
            new($"{typeof(T).Name.ToUpperInvariant()}_NOT_FOUND", $"No se encontró el registro solicitado ({id}).");

        public static Error Duplicate(string code, string message) => Error.Conflict(code, message);

        public static readonly Error UserNotRegistered =
            Error.Forbidden("USER_NOT_REGISTERED", "Tu usuario no está dado de alta en la planta.");

        public static readonly Error UserInactive =
            Error.Forbidden("USER_INACTIVE", "Tu usuario está inactivo.");

        public static readonly Error ReadOnlyUser =
            Error.Forbidden("USER_READ_ONLY", "El rol de auditor es de solo consulta y no puede firmar.");

        public static readonly Error NotAnInspector =
            Error.Forbidden("USER_NOT_INSPECTOR", "Tu usuario no está registrado como inspector.");

        public static readonly Error ApproverRequired =
            Error.Forbidden("APPROVER_REQUIRED", "Solo el administrador o el gerente de calidad pueden aprobar.");
    }
}
