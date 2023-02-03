using System.Collections.ObjectModel;

namespace HelpDeskSystem.Constants;

public static class PermissionNames
{
    public static readonly ReadOnlyCollection<string> AllPermissions = new List<string>
    {
        UsersPermissions.Update,
        UsersPermissions.Delete
        
    }.AsReadOnly();
        
    public static class UsersPermissions
    {
        private const string Users = "users.";
        public const string Update = Users + "update";
        public const string Delete = Users + "delete";
    }
}