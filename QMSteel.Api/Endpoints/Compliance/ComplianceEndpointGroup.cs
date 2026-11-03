namespace QMSteel.Api.Endpoints.Compliance
{
    public static class ComplianceEndpointGroup
    {
        public static RouteGroupBuilder MapComplianceEndpoints(this RouteGroupBuilder group)
        {
            var complianceGroup = group.MapGroup("").WithTags("Cumplimiento Normativo");
            complianceGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapGetComplianceMatrix()
                 .MapAssessComplianceRequirement()
                 .MapAddComplianceEvidence();
            return group;
        }
    }
}
