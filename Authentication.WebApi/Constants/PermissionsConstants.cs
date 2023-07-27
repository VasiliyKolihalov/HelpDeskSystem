namespace Authentication.WebApi.Constants;

public static class PermissionsConstants
{
    public static class AccountsPermissions
    {
        private const string Account = "accounts";
        public const string AddRole = Account + ".addtorole";
        public const string RemoveRole = Account + ".removefromrole";

        public static readonly IEnumerable<string> AllPermissions = new[]
        {
            AddRole,
            RemoveRole
        };
    }

    public static class RolesPermissions
    {
        private const string Roles = "roles";
        public const string Create = Roles + ".create";
        public const string Update = Roles + ".update";
        public const string Delete = Roles + ".delete";

        public static readonly IEnumerable<string> AllPermissions = new[]
        {
            Create,
            Update,
            Delete
        };
    }
    
  
    public static readonly IEnumerable<string> AllPermissions =
        AccountsPermissions.AllPermissions
            .Concat(RolesPermissions.AllPermissions);
}