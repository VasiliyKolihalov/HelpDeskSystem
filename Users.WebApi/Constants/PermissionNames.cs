namespace Users.WebApi.Constants;

public static class PermissionNames
{
    public static class UserPermissions
    {
        private const string Users = "users";
        public const string Create = Users + ".create";
        public const string Update = Users + ".update";
        public const string Delete = Users + ".delete";

        public static readonly IEnumerable<string> PolicyPermissions = new[]
        {
            Create,
            Update,
            Delete
        };
    }

    public static readonly IEnumerable<string> AllPolicyPermissions = UserPermissions.PolicyPermissions;
}