using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Common;
using QMSteel.Application.NonConformities;

namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class RegisterNonConformityCostEndpoint
    {
        private const string ENDPOINT_NAME = "Registrar Costo de No Calidad";

        public static RouteGroupBuilder MapRegisterNonConformityCost(this RouteGroupBuilder app)
        {
            app.MapPut("no-conformidades/{id:guid}/costo", async (Guid id, [FromBody] MoneyDto request, NonConformityService nonConformityService, CancellationToken ct) =>
            {
                var result = await nonConformityService.RegisterCostAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<NonConformityDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Registra el costo de no calidad de la NC en USD o MXN")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
