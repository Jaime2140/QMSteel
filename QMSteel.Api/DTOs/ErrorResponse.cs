namespace QMSteel.Api.DTOs
{
    // Cuerpo de todas las respuestas de error. El front usa Code para el modal rojo de regla violada (RNF 3.2.5).
    public sealed record ErrorResponse(int Status, string Error, string Code, string Message);
}
