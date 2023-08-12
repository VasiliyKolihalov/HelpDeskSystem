using Authentication.Infrastructure.Constants;
using Microsoft.AspNetCore.Authorization;

namespace Authentication.Infrastructure.Extensions;

public static class AuthorizationOptionsExtensions
{
    public static AuthorizationOptions AddPoliticiansBasedOnJwtPermissions(
        this AuthorizationOptions @this,
        IEnumerable<string> permissionNames)
    {
        foreach (string permission in permissionNames)
        {
            @this.AddPolicy(
                name: permission,
                configurePolicy: builder => { builder.RequireClaim(CustomJwtClaimTypes.Permission, permission); });
        }

        return @this;
    }

    public static AuthorizationOptions AddEmailConfirmPolicy(
        this AuthorizationOptions @this)
    {
        @this.AddPolicy(
            name: AccountPolicyNames.EmailConfirm,
            configurePolicy: builder =>
            {
                builder.RequireClaim(
                    claimType: CustomJwtClaimTypes.IsEmailConfirm,
                    allowedValues: true.ToString());
            });

        return @this;
    }
}