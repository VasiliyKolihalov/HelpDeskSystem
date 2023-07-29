using System.Security.Claims;
using Authentication.Infrastructure.Constants;
using Authentication.Infrastructure.Models;
using Microsoft.AspNetCore.Mvc;

namespace Authentication.Infrastructure.Extensions;

public static class ControllerBaseExtensions
{
    public static Account<TId> GetAccountFromJwt<TId>(this ControllerBase controllerBase) where TId : notnull
    {
        IEnumerable<Claim> userClaims = controllerBase.User.Claims.ToList();
        Claim idClaim = userClaims.Single(_ => _.Type == CustomJwtClaimTypes.Id);
        IEnumerable<Claim> roleClaims = userClaims.Where(_ => _.Type == CustomJwtClaimTypes.Role);
        IEnumerable<Claim> permissionClaims = userClaims.Where(_ => _.Type == CustomJwtClaimTypes.Permission);

        return new Account<TId>
        {
            Id = (TId)ChangeIdType<TId>(idClaim.Value),
            Roles = roleClaims.Select(_ => new Role { Id = _.Value }),
            Permissions = permissionClaims.Select(_ => new Permission { Id = _.Value })
        };
    }

    private static object ChangeIdType<TId>(string value)
    {
        return typeof(TId) == typeof(Guid) ? Guid.Parse(value) : Convert.ChangeType(value, typeof(TId));
    }
}