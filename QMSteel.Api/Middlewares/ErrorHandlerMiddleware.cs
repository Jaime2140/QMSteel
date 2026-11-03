using Microsoft.EntityFrameworkCore;
using QMSteel.Api.DTOs;
using QMSteel.Domain.Execution;

namespace QMSteel.Api.Middlewares
{
    public class ErrorHandlerMiddleware
    {
        private readonly ILogger<ErrorHandlerMiddleware> _logger;
        private readonly RequestDelegate _next;

        public ErrorHandlerMiddleware(RequestDelegate next, ILogger<ErrorHandlerMiddleware> logger)
        {
            _next = next;
            _logger = logger;
        }

        public async Task InvokeAsync(HttpContext context)
        {
            try
            {
                await _next(context);
            }
            catch (DomainException ex)
            {
                // Regla de negocio violada (ej. soldador vencido): el front la muestra en el modal rojo.
                _logger.LogWarning("Regla de negocio {Code}: {Message}", ex.Code, ex.Message);
                await WriteAsync(context, StatusCodes.Status422UnprocessableEntity, "Regla de negocio", ex.Code, ex.Message);
            }
            catch (BadHttpRequestException ex)
            {
                _logger.LogWarning(ex, "Solicitud inválida");
                await WriteAsync(context, StatusCodes.Status400BadRequest, "Solicitud inválida", "BAD_REQUEST", ex.Message);
            }
            catch (DbUpdateConcurrencyException ex)
            {
                _logger.LogWarning(ex, "Conflicto de edición");
                await WriteAsync(context, StatusCodes.Status409Conflict, "Conflicto de edición", "CONCURRENCY_CONFLICT",
                    "Otro usuario modificó este registro. Recarga la información e intenta de nuevo.");
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno");
                await WriteAsync(context, StatusCodes.Status500InternalServerError, "Error interno", "INTERNAL_ERROR",
                    "Ocurrió un error inesperado.");
            }
        }

        private static Task WriteAsync(HttpContext context, int status, string error, string code, string message)
        {
            context.Response.StatusCode = status;
            return context.Response.WriteAsJsonAsync(new ErrorResponse(status, error, code, message));
        }
    }
}
