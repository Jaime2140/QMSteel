using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class AddWelderQualificationEndpoint
    {
        private const string ENDPOINT_NAME = "Agregar Calificacion de Soldador";

        public static RouteGroupBuilder MapAddWelderQualification(this RouteGroupBuilder app)
        {
            app.MapPost("soldadores/{id:guid}/calificaciones", async (Guid id, [FromBody] AddQualificationRequest request, WelderService welderService, CancellationToken ct) =>
            {
                var result = await welderService.AddQualificationAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<WelderDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra una calificación (WPQR) por proceso y posiciones")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
