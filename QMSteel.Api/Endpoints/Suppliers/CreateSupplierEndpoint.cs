using Microsoft.AspNetCore.Mvc;
using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Suppliers
{
    public static class CreateSupplierEndpoint
    {
        private const string ENDPOINT_NAME = "Crear Proveedor";

        public static RouteGroupBuilder MapCreateSupplier(this RouteGroupBuilder app)
        {
            app.MapPost("proveedores", async ([FromBody] CreateSupplierRequest request, SupplierService supplierService, CancellationToken ct) =>
            {
                var result = await supplierService.CreateAsync(request, ct);
                return result.ToCreatedResult(supplier => $"/api/proveedores/{supplier.Id}");
            })
            .Produces<SupplierDto>(StatusCodes.Status201Created)
            .Produces<ErrorResponse>(StatusCodes.Status409Conflict)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Da de alta un proveedor como prospecto de la AVL")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
