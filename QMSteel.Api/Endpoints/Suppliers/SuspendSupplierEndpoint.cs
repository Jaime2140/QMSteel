using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Suppliers
{
    public static class SuspendSupplierEndpoint
    {
        private const string ENDPOINT_NAME = "Suspender Proveedor";

        public static RouteGroupBuilder MapSuspendSupplier(this RouteGroupBuilder app)
        {
            app.MapPost("proveedores/{id:guid}/suspender", async (Guid id, [FromBody] ReasonRequest request, SupplierService supplierService, CancellationToken ct) =>
            {
                var result = await supplierService.SuspendAsync(id, request.Reason, ct);
                return result.ToHttpResult();
            })
            .Produces<SupplierDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Suspende al proveedor indicando el motivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
