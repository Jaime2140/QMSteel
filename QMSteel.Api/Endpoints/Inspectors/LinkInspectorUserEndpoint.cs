using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Personnel;

namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class LinkInspectorUserEndpoint
    {
        private const string ENDPOINT_NAME = "Ligar Usuario a Inspector";

        public static RouteGroupBuilder MapLinkInspectorUser(this RouteGroupBuilder app)
        {
            app.MapPut("inspectores/{id:guid}/usuario/{userId:guid}", async (Guid id, Guid userId, InspectorService inspectorService, CancellationToken ct) =>
            {
                var result = await inspectorService.LinkUserAsync(id, userId, ct);
                return result.ToHttpResult();
            })
            .Produces<InspectorDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Liga el inspector con el usuario con el que firma")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
