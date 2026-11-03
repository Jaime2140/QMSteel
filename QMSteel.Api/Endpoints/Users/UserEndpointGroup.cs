namespace QMSteel.Api.Endpoints.Users
{
    public static class UserEndpointGroup
    {
        public static RouteGroupBuilder MapUserEndpoints(this RouteGroupBuilder group)
        {
            var userGroup = group.MapGroup("").WithTags("Usuarios");
            userGroup.MapEndpoints();
            return group;
        }

        private static RouteGroupBuilder MapEndpoints(this RouteGroupBuilder group)
        {
            group.MapGetCurrentUser()
                 .MapListUsers()
                 .MapGetUser()
                 .MapCreateUser()
                 .MapUpdateUser()
                 .MapChangeUserRole()
                 .MapDeactivateUser()
                 .MapActivateUser();
            return group;
        }
    }
}
