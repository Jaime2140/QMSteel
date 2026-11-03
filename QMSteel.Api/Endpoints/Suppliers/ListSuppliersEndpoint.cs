using Microsoft.AspNetCore.Mvc;
using QMSteel.Application.Suppliers;
using QMSteel.Domain.Entities.Suppliers;

namespace QMSteel.Api.Endpoints.Suppliers
{
    public static class ListSuppliersEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Proveedores";

        public static RouteGroupBuilder MapListSuppliers(this RouteGroupBuilder app)
        {
            app.MapGet("proveedores", async ([FromQuery(Name = "estatus")] SupplierStatus? status, [FromQuery(Name = "categoria")] SupplierCategory? category, SupplierService supplierService, CancellationToken ct) =>
            {
                var response = await supplierService.ListAsync(status, category, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<SupplierDto>>(StatusCodes.Status200OK)
            .WithDescription("Directorio AVL con score y estatus, filtrable por estatus y categoría")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
