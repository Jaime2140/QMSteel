using System.Security.Claims;
using QMSteel.Api.DTOs;
using QMSteel.Api.Security;
using QMSteel.Domain.Repositories;

namespace QMSteel.Api.Middlewares
{
    // Traduce la identidad del token (claim "sub") al usuario y la planta de QMSteel.
    // Bloquea usuarios no registrados o inactivos, plantas sin suscripción vigente y escrituras del rol auditor.
    public class CurrentUserMiddleware
    {
        private readonly RequestDelegate _next;

        public CurrentUserMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, HttpCurrentUserContext currentUser, IUserRepository users,
            ITenantRepository tenants, TimeProvider time)
        {
            if (context.User.Identity?.IsAuthenticated != true)
            {
                await _next(context);
                return;
            }

            var identityId = context.User.FindFirstValue(ClaimTypes.NameIdentifier);
            var user = string.IsNullOrEmpty(identityId) ? null : await users.GetByIdentityIdAsync(identityId, context.RequestAborted);
            if (user is null)
            {
                await ForbidAsync(context, "USER_NOT_REGISTERED", "Tu usuario no está dado de alta en ninguna planta.");
                return;
            }
            if (!user.IsActive)
            {
                await ForbidAsync(context, "USER_INACTIVE", "Tu usuario está inactivo.");
                return;
            }

            var tenant = await tenants.GetByIdAsync(user.TenantId, context.RequestAborted);
            if (tenant is null || !tenant.CanOperate(DateOnly.FromDateTime(time.GetLocalNow().DateTime)))
            {
                await ForbidAsync(context, "TENANT_SUBSCRIPTION_INACTIVE", "La suscripción de la planta no está vigente.");
                return;
            }

            if (user.IsReadOnly && !HttpMethods.IsGet(context.Request.Method) && !HttpMethods.IsHead(context.Request.Method))
            {
                await ForbidAsync(context, "USER_READ_ONLY", "El rol de auditor es de solo consulta.");
                return;
            }

            currentUser.SignIn(user);
            await _next(context);
        }

        private static Task ForbidAsync(HttpContext context, string code, string message)
        {
            context.Response.StatusCode = StatusCodes.Status403Forbidden;
            return context.Response.WriteAsJsonAsync(new ErrorResponse(StatusCodes.Status403Forbidden, "Acceso denegado", code, message));
        }
    }
}
