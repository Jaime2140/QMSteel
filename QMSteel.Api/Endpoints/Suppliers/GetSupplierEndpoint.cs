using QMSteel.Api.DTOs;
using QMSteel.Api.Extensions;
using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Suppliers
{
    public static class GetSupplierEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Proveedor";

        public static RouteGroupBuilder MapGetSupplier(this RouteGroupBuilder app)
        {
            app.MapGet("proveedores/{id:guid}", async (Guid id, SupplierService supplierService, CancellationToken ct) =>
            {
                var result = await supplierService.GetAsync(id, ct);
                return result.ToHttpResult();
            })
            .Produces<SupplierDto>(StatusCodes.Status200OK)
            .Produces<ErrorResponse>(StatusCodes.Status404NotFound)
            .WithDescription("Detalle del proveedor con su historial de evaluaciones")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
