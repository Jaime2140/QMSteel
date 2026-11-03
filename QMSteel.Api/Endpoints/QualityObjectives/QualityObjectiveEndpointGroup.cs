namespace QMSteel.Api.Endpoints.QualityObjectives
{
    public static class QualityObjectiveEndpointGroup
    {
        public static RouteGroupBuilder MapQualityObjectiveEndpoints(this RouteGroupBuilder group)
        {
            var qualityObjectiveGroup = group.MapGroup("").WithTags("Objetivos de Calidad");
            qualityObjectiveGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListQualityObjectives()
                 .MapGetQualityObjective()
                 .MapCreateQualityObjective()
                 .MapChangeQualityObjectiveTarget()
                 .MapRecordQualityObjectiveMeasurement();
            return group;
        }
    }
}
