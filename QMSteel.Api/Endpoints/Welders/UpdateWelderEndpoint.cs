using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class UpdateWelderEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar Soldador";

        public static RouteGroupBuilder MapUpdateWelder(this RouteGroupBuilder app)
        {
            app.MapPut("soldadores/{id:guid}", async (Guid id, [FromBody] UpdateWelderRequest request, WelderService welderService, CancellationToken ct) =>
            {
                var result = await welderService.UpdateAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<WelderDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cambia el nombre, la estampa o el tipo del soldador")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
