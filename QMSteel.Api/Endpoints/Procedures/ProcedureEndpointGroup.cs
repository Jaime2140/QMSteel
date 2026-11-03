namespace QMSteel.Api.Endpoints.Procedures
{
    public static class ProcedureEndpointGroup
    {
        public static RouteGroupBuilder MapProcedureEndpoints(this RouteGroupBuilder group)
        {
            var procedureGroup = group.MapGroup("").WithTags("WPS y PQR");
            procedureGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListPqrs()
                 .MapGetPqr()
                 .MapCreatePqr()
                 .MapUpdatePqrVariables()
                 .MapAttachPqrFile()
                 .MapApprovePqr()
                 .MapMakePqrObsolete()
                 .MapListWps()
                 .MapGetWpsMatrix()
                 .MapGetWps()
                 .MapCreateWps()
                 .MapUpdateWps()
                 .MapAttachWpsFile()
                 .MapLinkPqrToWps()
                 .MapUnlinkPqrFromWps()
                 .MapSubmitWpsForReview()
                 .MapReturnWpsToDraft()
                 .MapApproveWps()
                 .MapStartWpsRevision()
                 .MapMakeWpsObsolete();
            return group;
        }
    }
}
