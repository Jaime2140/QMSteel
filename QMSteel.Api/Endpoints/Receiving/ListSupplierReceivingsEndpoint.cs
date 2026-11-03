using QMSteel.Application.Suppliers;

namespace QMSteel.Api.Endpoints.Receiving
{
    public static class ListSupplierReceivingsEndpoint
    {
        private const string ENDPOINT_NAME = "Consultar Recepciones de Proveedor";

        public static RouteGroupBuilder MapListSupplierReceivings(this RouteGroupBuilder app)
        {
            app.MapGet("proveedores/{supplierId:guid}/recepciones", async (Guid supplierId, ReceivingService receivingService, CancellationToken ct) =>
            {
                var response = await receivingService.ListBySupplierAsync(supplierId, ct);
                return Results.Ok(response);
            })
            .Produces<IReadOnlyList<ReceivingDto>>(StatusCodes.Status200OK)
            .WithDescription("Recepciones de material del proveedor")
            .WithSummary(ENDPOINT_NAME)
            .WithName(ENDPOINT_NAME);
            return app;
        }
    }
}
