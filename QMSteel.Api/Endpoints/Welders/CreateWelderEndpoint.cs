using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class CreateWelderEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Soldador";

        public static RouteGroupBuilder MapCreateWelder(this RouteGroupBuilder app)
        {
            app.MapPost("soldadores", async ([FromBody] CreateWelderRequest request, WelderService welderService, CancellationToken ct) =>
            {
                var result = await welderService.CreateAsync(request, ct);
                return result.ToCreatedResult(welder => $"/api/soldadores/{welder.Id}");
            })
            .Produces<WelderDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Da de alta un soldador o punteador con su estampa")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
