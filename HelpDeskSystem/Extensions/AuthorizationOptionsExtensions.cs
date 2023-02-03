using Microsoft.AspNetCore.Authorization;

namespace HelpDeskSystem.Extensions;

public static class AuthorizationOptionsExtensions
{
    public static void AddPoliticiansBasedOnPermissions(
        this AuthorizationOptions @this,
        IEnumerable<string> permissions,
        string permissionClaimType)
    {
        if (permissions == null) throw new ArgumentNullException(nameof(permissions));
        if (permissionClaimType == null) throw new ArgumentNullException(nameof(permissionClaimType));
        
        foreach (string permission in permissions)
        {
            @this.AddPolicy(permission, policyBuilder =>
            {
                policyBuilder.RequireClaim(permissionClaimType, permission);
            });
        }
    }
}