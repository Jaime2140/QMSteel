namespace QMSteel.Api.Endpoints.Dashboard
{
    public static class DashboardEndpointGroup
    {
        public static RouteGroupBuilder MapDashboardEndpoints(this RouteGroupBuilder group)
        {
            var dashboardGroup = group.MapGroup("").WithTags("Tablero");
            dashboardGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapGetExecutiveDashboard();
            return group;
        }
    }
}
