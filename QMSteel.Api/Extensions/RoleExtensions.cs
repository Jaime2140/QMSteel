using QMSteel.Api.DTOs;
using QMSteel.Api.Security;
using QMSteel.Domain.Entities.Users;

namespace QMSteel.Api.Extensions
{
    public static class RoleExtensions
    {
        // Los roles viven en QMSteel, no en el token: se valida con el usuario que resolvió CurrentUserMiddleware.
        public static TBuilder RequireRoles<TBuilder>(this TBuilder builder, params UserRole[] roles) where TBuilder : IEndpointConventionBuilder =>
            builder.AddEndpointFilter(async (context, next) =>
            {
                var currentUser = context.HttpContext.RequestServices.GetRequiredService<HttpCurrentUserContext>();
                if (currentUser.Role is { } role && roles.Contains(role))
                    return await next(context);

                return Results.Json(new ErrorResponse(StatusCodes.Status403Forbidden, "Acción no permitida", "ROLE_NOT_ALLOWED",
                    "Tu rol no tiene permiso para esta operación."), statusCode: StatusCodes.Status403Forbidden);
            });
    }
}
