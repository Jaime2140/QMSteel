namespace QMSteel.Api.Endpoints.NonConformities
{
    public static class NonConformityEndpointGroup
    {
        public static RouteGroupBuilder MapNonConformityEndpoints(this RouteGroupBuilder group)
        {
            var nonConformityGroup = group.MapGroup("").WithTags("No Conformidades");
            nonConformityGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListOpenNonConformities()
                 .MapListNonConformities()
                 .MapGetNonConformityMonitor()
                 .MapGetNonConformity()
                 .MapOpenNonConformity()
                 .MapUpdateNonConformity()
                 .MapAddNonConformityEvidence()
                 .MapSetNonConformityDisposition()
                 .MapAssignNonConformityCommittee()
                 .MapRecordNonConformityRootCause()
                 .MapAddNonConformityAction()
                 .MapCompleteNonConformityAction()
                 .MapRescheduleNonConformityAction()
                 .MapRegisterNonConformityCost()
                 .MapCloseNonConformity()
                 .MapCancelNonConformity();
            return group;
        }
    }
}
