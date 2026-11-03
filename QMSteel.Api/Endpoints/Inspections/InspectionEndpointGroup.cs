namespace QMSteel.Api.Endpoints.Inspections
{
    public static class InspectionEndpointGroup
    {
        public static RouteGroupBuilder MapInspectionEndpoints(this RouteGroupBuilder group)
        {
            var inspectionGroup = group.MapGroup("").WithTags("Inspecciones");
            inspectionGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapGetInspection()
                 .MapListInspectionsByProject()
                 .MapListSignedInspections()
                 .MapGetInspectionKpis()
                 .MapCreateInspection()
                 .MapAssignInspectionWps()
                 .MapAddInspectionEquipment()
                 .MapRemoveInspectionEquipment()
                 .MapAddInspectionWeldResult()
                 .MapAddInspectionCoatingMeasurement()
                 .MapSetInspectionCleanlinessStandard()
                 .MapRemoveInspectionItem()
                 .MapAddInspectionPhoto()
                 .MapSetInspectionNotes()
                 .MapSignInspection()
                 .MapVoidInspection();
            return group;
        }
    }
}
