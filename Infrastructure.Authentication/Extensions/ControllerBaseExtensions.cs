using System.Security.Claims;
using Infrastructure.Authentication.Constants;
using Infrastructure.Authentication.Models;
using Microsoft.AspNetCore.Mvc;

namespace Infrastructure.Authentication.Extensions;

public static class ControllerBaseExtensions
{
    public static Account<TId> GetAccountFromJwt<TId>(this ControllerBase controllerBase) where TId : notnull
    {
        IEnumerable<Claim> userClaims = controllerBase.User.Claims.ToList();
        Claim idClaim = userClaims.Single(_ => _.Type == CustomJwtClaimTypes.Id);
        Claim isEmailConfirmClaim = userClaims.Single(_ => _.Type == CustomJwtClaimTypes.IsEmailConfirm);
        IEnumerable<Claim> roleClaims = userClaims.Where(_ => _.Type == CustomJwtClaimTypes.Role);
        IEnumerable<Claim> permissionClaims = userClaims.Where(_ => _.Type == CustomJwtClaimTypes.Permission);

        return new Account<TId>
        {
            Id = ChangeIdType<TId>(idClaim.Value),
            IsEmailConfirm = bool.Parse(isEmailConfirmClaim.Value),
            Roles = roleClaims.Select(_ => new Role { Id = _.Value }),
            Permissions = permissionClaims.Select(_ => new Permission { Id = _.Value })
        };
    }

    public static TId GetAccountIdFromJwt<TId>(this ControllerBase controllerBase) where TId : notnull
    {
        IEnumerable<Claim> userClaims = controllerBase.User.Claims.ToList();
        Claim idClaim = userClaims.Single(_ => _.Type == CustomJwtClaimTypes.Id);
        return ChangeIdType<TId>(idClaim.Value);
    }

    private static TId ChangeIdType<TId>(string value)
    {
        return (TId)(typeof(TId) == typeof(Guid) ? Guid.Parse(value) : Convert.ChangeType(value, typeof(TId)));
    }
}