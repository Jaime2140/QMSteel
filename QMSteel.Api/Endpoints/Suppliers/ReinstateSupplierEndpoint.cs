using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Suppliers
{
    public static class ReinstateSupplierEndpoint
    {
        private const string ENDPOINT_NAME = "Reactivar Proveedor";

        public static RouteGroupBuilder MapReinstateSupplier(this RouteGroupBuilder app)
        {
            app.MapPost("proveedores/{id:guid}/reactivar", async (Guid id, SupplierService supplierService, CancellationToken ct) =>
            {
                var result = await supplierService.ReinstateAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<SupplierDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .Produces<ErrorResponse>(StatusCodes.Status422UnprocessableEntity)
            .WithDescription("Reactiva a un proveedor suspendido")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
