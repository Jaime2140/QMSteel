namespace QMSteel.Api.Endpoints.Welders
{
    public static class WelderEndpointGroup
    {
        public static RouteGroupBuilder MapWelderEndpoints(this RouteGroupBuilder group)
        {
            var welderGroup = group.MapGroup("").WithTags("Soldadores");
            welderGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListWelders()
                 .MapListEligibleWelders()
                 .MapGetWelderKpis()
                 .MapGetWelder()
                 .MapGetWelderHistory()
                 .MapCreateWelder()
                 .MapUpdateWelder()
                 .MapAddWelderQualification()
                 .MapAttachQualificationCertificate()
                 .MapRevokeWelderQualification()
                 .MapDeactivateWelder()
                 .MapActivateWelder();
            return group;
        }
    }
}
