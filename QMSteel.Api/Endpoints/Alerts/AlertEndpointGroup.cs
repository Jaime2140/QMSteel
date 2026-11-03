namespace QMSteel.Api.Endpoints.Alerts
{
    public static class AlertEndpointGroup
    {
        public static RouteGroupBuilder MapAlertEndpoints(this RouteGroupBuilder group)
        {
            var alertGroup = group.MapGroup("").WithTags("Alertas");
            alertGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapGetExpiryAlerts();
            return group;
        }
    }
}
