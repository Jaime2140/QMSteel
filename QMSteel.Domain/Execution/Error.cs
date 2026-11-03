namespace QMSteel.Domain.Execution;

// Clasifica el error para que quien lo reciba (la API) decida cómo responder sin interpretar el código.
public enum ErrorType { Failure, Validation, NotFound, Conflict, Forbidden }

public sealed record Error(string Code, string Message, ErrorType Type = ErrorType.Failure)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entity, Guid id) =>
        new($"{entity.ToUpperInvariant()}_NOT_FOUND", $"{entity} con id '{id}' no existe.", ErrorType.NotFound);

    public static Error Validation(string code, string message) => new(code, message, ErrorType.Validation);

    public static Error Conflict(string code, string message) => new(code, message, ErrorType.Conflict);

    public static Error Forbidden(string code, string message) => new(code, message, ErrorType.Forbidden);
}
