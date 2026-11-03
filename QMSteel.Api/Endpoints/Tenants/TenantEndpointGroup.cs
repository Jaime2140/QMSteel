namespace QMSteel.Api.Endpoints.Tenants
{
    public static class TenantEndpointGroup
    {
        public static RouteGroupBuilder MapTenantEndpoints(this RouteGroupBuilder group)
        {
            var tenantGroup = group.MapGroup("").WithTags("Planta");
            tenantGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapGetCurrentTenant()
                 .MapEnableQualityStandard()
                 .MapDisableQualityStandard();
            return group;
        }
    }
}
