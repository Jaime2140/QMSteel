namespace QMSteel.Api.Endpoints.Receiving
{
    public static class ReceivingEndpointGroup
    {
        public static RouteGroupBuilder MapReceivingEndpoints(this RouteGroupBuilder group)
        {
            var receivingGroup = group.MapGroup("").WithTags("Inspeccion de Recibo");
            receivingGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapGetReceiving()
                 .MapListSupplierReceivings()
                 .MapListReceivingsByHeatNumber()
                 .MapStartReceiving()
                 .MapAddReceivedItem()
                 .MapUpdateReceivedItemMtr()
                 .MapRecordReceivingVisualInspection()
                 .MapCompleteReceiving();
            return group;
        }
    }
}
