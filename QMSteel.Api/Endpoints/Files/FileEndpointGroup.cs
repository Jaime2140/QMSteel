namespace QMSteel.Api.Endpoints.Files
{
    public static class FileEndpointGroup
    {
        public static RouteGroupBuilder MapFileEndpoints(this RouteGroupBuilder group)
        {
            var fileGroup = group.MapGroup("").WithTags("Archivos");
            fileGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapUploadFile()
                 .MapDownloadFile();
            return group;
        }
    }
}
