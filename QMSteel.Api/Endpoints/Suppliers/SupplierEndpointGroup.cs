namespace QMSteel.Api.Endpoints.Suppliers
{
    public static class SupplierEndpointGroup
    {
        public static RouteGroupBuilder MapSupplierEndpoints(this RouteGroupBuilder group)
        {
            var supplierGroup = group.MapGroup("").WithTags("Proveedores");
            supplierGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListSuppliers()
                 .MapGetSupplier()
                 .MapCreateSupplier()
                 .MapUpdateSupplier()
                 .MapEvaluateSupplier()
                 .MapSuspendSupplier()
                 .MapReinstateSupplier()
                 .MapRemoveSupplier();
            return group;
        }
    }
}
