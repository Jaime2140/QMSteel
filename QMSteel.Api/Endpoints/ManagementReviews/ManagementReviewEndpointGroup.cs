namespace QMSteel.Api.Endpoints.ManagementReviews
{
    public static class ManagementReviewEndpointGroup
    {
        public static RouteGroupBuilder MapManagementReviewEndpoints(this RouteGroupBuilder group)
        {
            var managementReviewGroup = group.MapGroup("").WithTags("Revision por la Direccion");
            managementReviewGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListManagementReviews()
                 .MapGetManagementReview()
                 .MapCreateManagementReview()
                 .MapSetManagementReviewAttendees()
                 .MapAttachManagementReviewMinutes()
                 .MapAttachManagementReviewRiskAnalysis()
                 .MapAddManagementReviewDecision()
                 .MapCompleteManagementReviewDecision()
                 .MapApproveManagementReview();
            return group;
        }
    }
}
