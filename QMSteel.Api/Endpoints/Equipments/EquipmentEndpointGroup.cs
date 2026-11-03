namespace QMSteel.Api.Endpoints.Equipments
{
    public static class EquipmentEndpointGroup
    {
        public static RouteGroupBuilder MapEquipmentEndpoints(this RouteGroupBuilder group)
        {
            var equipmentGroup = group.MapGroup("").WithTags("Equipos");
            equipmentGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapListEquipments()
                 .MapListEligibleEquipments()
                 .MapListCalibrationDue()
                 .MapGetEquipment()
                 .MapCreateEquipment()
                 .MapUpdateEquipment()
                 .MapRegisterCalibration()
                 .MapAttachCalibrationCertificate()
                 .MapRegisterVerification()
                 .MapTakeEquipmentOutOfService()
                 .MapReturnEquipmentToService()
                 .MapRetireEquipment();
            return group;
        }
    }
}
