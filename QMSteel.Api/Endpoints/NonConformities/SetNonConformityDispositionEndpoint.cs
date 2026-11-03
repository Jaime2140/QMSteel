using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class SetNonConformityDispositionEndpoint
    {
        private const string ENDPOINT_NAME = "Definir Disposicion de NC";

        public static RouteGroupBuilder MapSetNonConformityDisposition(this RouteGroupBuilder app)
        {
            app.MapPut("no-conformidades/{id:guid}/disposicion", async (Guid id, [FromBody] DispositionRequest request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.SetDispositionAsync(id, request.Disposition, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Define la disposición del producto no conforme")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
