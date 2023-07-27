namespace Users.WebApi.Constants;

public static class PermissionsConstants
{
    public static class UsersPermissions
    {
        private const string Users = "users";
        public const string Create = Users + ".create";
        public const string Update = Users + ".update";
        public const string Delete = Users + ".delete";

        public static readonly IEnumerable<string> AllPermissions = new[]
        {
            Create,
            Update,
            Delete
        };
    }

    public static readonly IEnumerable<string> AllPermissions = UsersPermissions.AllPermissions;
}