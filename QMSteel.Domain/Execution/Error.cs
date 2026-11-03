namespace QMSteel.Domain.Execution;

public sealed record Error(string Code, string Message)
{
    public static readonly Error None = new(string.Empty, string.Empty);

    public static Error NotFound(string entity, Guid id) =>
        new($"{entity.ToUpperInvariant()}_NOT_FOUND", $"{entity} con id '{id}' no existe.");

    public static Error Validation(string code, string message) => new(code, message);

    public static Error Conflict(string code, string message) => new(code, message);

    public static Error Forbidden(string code, string message) => new(code, message);
}