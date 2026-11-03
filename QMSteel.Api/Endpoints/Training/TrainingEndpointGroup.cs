namespace QMSteel.Api.Endpoints.Training
{
    public static class TrainingEndpointGroup
    {
        public static RouteGroupBuilder MapTrainingEndpoints(this RouteGroupBuilder group)
        {
            var trainingGroup = group.MapGroup("").WithTags("Capacitacion");
            trainingGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapGetTrainingPlan()
                 .MapCreateTrainingPlan()
                 .MapAddPlannedCourse()
                 .MapCancelPlannedCourse()
                 .MapListTrainingSessions()
                 .MapListEmployeeTrainingSessions()
                 .MapGetTrainingSession()
                 .MapCreateTrainingSession()
                 .MapAddTrainingAttendee()
                 .MapRemoveTrainingAttendee()
                 .MapRecordTrainingAttendance()
                 .MapMarkTrainingSessionHeld()
                 .MapEvaluateTrainingEffectiveness()
                 .MapIssueTrainingCertificate()
                 .MapCloseTrainingSession()
                 .MapCancelTrainingSession();
            return group;
        }
    }
}
