namespace Authentication.WebApi.Constants;

public static class PermissionNames
{
    public static class AccountPermissions
    {
        private const string Account = "accounts";
        public const string AddRole = Account + ".addtorole";
        public const string RemoveRole = Account + ".removefromrole";

        public static readonly IEnumerable<string> PolicyPermissions = new[]
        {
            AddRole,
            RemoveRole
        };
    }

    public static class RolePermissions
    {
        private const string Roles = "roles";
        public const string Create = Roles + ".create";
        public const string Update = Roles + ".update";
        public const string Delete = Roles + ".delete";

        public static readonly IEnumerable<string> PolicyPermissions = new[]
        {
            Create,
            Update,
            Delete
        };
    }

    public static class HttpClientPermissions
    {
        public const string UsersCreate = "users.create";
        public const string UsersDelete = "users.delete";
    }

    public static readonly IEnumerable<string> AllPolicyPermissions =
        AccountPermissions.PolicyPermissions
            .Concat(RolePermissions.PolicyPermissions);
}