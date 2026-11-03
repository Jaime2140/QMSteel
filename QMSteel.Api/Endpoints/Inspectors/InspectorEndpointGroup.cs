namespace QMSteel.Api.Endpoints.Inspectors
{
    public static class InspectorEndpointGroup
    {
        public static RouteGroupBuilder MapInspectorEndpoints(this RouteGroupBuilder group)
        {
            var inspectorGroup = group.MapGroup("").WithTags("Inspectores");
            inspectorGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListInspectors()
                 .MapGetCurrentInspector()
                 .MapGetInspector()
                 .MapCreateInspector()
                 .MapLinkInspectorUser()
                 .MapAddInspectorCertification()
                 .MapRegisterVisionExam()
                 .MapDeactivateInspector()
                 .MapActivateInspector();
            return group;
        }
    }
}
