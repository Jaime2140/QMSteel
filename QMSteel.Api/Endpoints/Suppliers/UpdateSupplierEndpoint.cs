using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Suppliers
{
    public static class UpdateSupplierEndpoint
    {
        private const string ENDPOINT_NAME = "Actualizar Proveedor";

        public static RouteGroupBuilder MapUpdateSupplier(this RouteGroupBuilder app)
        {
            app.MapPut("proveedores/{id:guid}", async (Guid id, [FromBody] UpdateSupplierRequest request, SupplierService supplierService, CancellationToken ct) =>
            {
                var result = await supplierService.UpdateAsync(id, request, ct);
                return result.ToHttpResult();
            })
            .Produces<SupplierDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Cambia los datos de contacto y las categorías del proveedor")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
