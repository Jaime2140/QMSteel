using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Welders;

namespace QMSteel.Api.Endpoints.Welders
{
    public static class GetWelderEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Soldador";

        public static RouteGroupBuilder MapGetWelder(this RouteGroupBuilder app)
        {
            app.MapGet("soldadores/{id:guid}", async (Guid id, WelderService welderService, CancellationToken ct) =>
            {
                var result = await welderService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<WelderDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Expediente del soldador con sus calificaciones y continuidad")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
