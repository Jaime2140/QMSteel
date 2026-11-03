using QMSteel.Api.DTOs;
using QMSteel.Domain.Execution;

namespace QMSteel.Api.Extensions
{
    // Convierte el Result de los casos de uso en la respuesta HTTP correspondiente.
    public static class ResultExtensions
    {
        public static IResult ToHttpResult<T>(this Result<T> result) =>
            result.IsSuccess ? Results.Ok(result.Value) : result.Error.ToHttpResult();

        public static IResult ToCreatedResult<T>(this Result<T> result, Func<T, string> location) =>
            result.IsSuccess ? Results.Created(location(result.Value), result.Value) : result.Error.ToHttpResult();

        public static IResult ToHttpResult(this Error error)
        {
            var (status, title) = error.Type switch
            {
                ErrorType.NotFound => (StatusCodes.Status404NotFound, "Registro no encontrado"),
                ErrorType.Conflict => (StatusCodes.Status409Conflict, "Registro duplicado"),
                ErrorType.Forbidden => (StatusCodes.Status403Forbidden, "Acción no permitida"),
                ErrorType.Validation => (StatusCodes.Status422UnprocessableEntity, "Regla de negocio"),
                _ => (StatusCodes.Status400BadRequest, "Solicitud inválida")
            };

            return Results.Json(new ErrorResponse(status, title, error.Code, error.Message), statusCode: status);
        }
    }
}
