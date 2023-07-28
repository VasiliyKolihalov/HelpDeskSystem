using Authentication.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Authentication.Infrastructure.Extensions;

public static class AuthorizationOptionsExtensions
{
    public static void AddPolicyBasedOnJwtPermissions(
        this AuthorizationOptions @this,
        IEnumerable<string> permissionNames)
    {
        foreach (string permission in permissionNames)
        {
            @this.AddPolicy(
                name: permission,
                configurePolicy: builder => { builder.RequireClaim(CustomJwtClaimTypes.Permission, permission); });
        }
    }
}