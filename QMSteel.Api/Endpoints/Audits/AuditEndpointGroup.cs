namespace QMSteel.Api.Endpoints.Audits
{
    public static class AuditEndpointGroup
    {
        public static RouteGroupBuilder MapAuditEndpoints(this RouteGroupBuilder group)
        {
            var auditGroup = group.MapGroup("").WithTags("Auditorias");
            auditGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListAudits()
                 .MapGetAudit()
                 .MapCreateAudit()
                 .MapAddAuditParticipant()
                 .MapRemoveAuditParticipant()
                 .MapScheduleAudit()
                 .MapRescheduleAudit()
                 .MapStartAudit()
                 .MapAddAuditFinding()
                 .MapCompleteAudit()
                 .MapCancelAudit();
            return group;
        }
    }
}
