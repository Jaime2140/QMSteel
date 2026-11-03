namespace QMSteel.Api.Endpoints.Documents
{
    public static class DocumentEndpointGroup
    {
        public static RouteGroupBuilder MapDocumentEndpoints(this RouteGroupBuilder group)
        {
            var documentGroup = group.MapGroup("").WithTags("Documentos");
            documentGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListDocuments()
                 .MapGetDocument()
                 .MapCreateDocument()
                 .MapUpdateDocument()
                 .MapStartDocumentRevision()
                 .MapReplacePendingRevisionFile()
                 .MapSubmitDocumentRevision()
                 .MapRejectDocumentRevision()
                 .MapApproveDocumentRevision()
                 .MapConfirmDocumentReview()
                 .MapIssueControlledCopy()
                 .MapRevokeControlledCopy()
                 .MapGetCopyWatermark()
                 .MapMakeDocumentObsolete();
            return group;
        }
    }
}
