using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Suppliers
{
    public static class RemoveSupplierEndpoint
    {
        private const string ENDPOINT_NAME = "Dar de Baja Proveedor";

        public static RouteGroupBuilder MapRemoveSupplier(this RouteGroupBuilder app)
        {
            app.MapPost("proveedores/{id:guid}/baja", async (Guid id, [FromBody] ReasonRequest request, SupplierService supplierService, CancellationToken ct) =>
            {
                var result = await supplierService.RemoveAsync(id, request.Reason, ct);
                return result.ToHttpResult();
            })
            .Produces<SupplierDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Da de baja al proveedor de la AVL indicando el motivo")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
