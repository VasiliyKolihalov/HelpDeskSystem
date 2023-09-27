using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Infrastructure.Authentication.Constants;
using Infrastructure.Authentication.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Infrastructure.Authentication.Services;

public class JwtService : IJwtService
{
    private readonly JwtAuthOptions _jwtAuthOptions;

    public JwtService(IOptions<JwtAuthOptions> jwtAuthOptions)
    {
        _jwtAuthOptions = jwtAuthOptions.Value;
    }

    public string GenerateJwt<TId>(Account<TId> account) where TId : notnull
    {
        if (account == null) throw new ArgumentNullException(nameof(account));

        SymmetricSecurityKey securityKey = _jwtAuthOptions.SymmetricSecurityKey;
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(
                type: CustomJwtClaimTypes.Id,
                value: account.Id.ToString()!),

            new(type: CustomJwtClaimTypes.IsEmailConfirm, value: account.IsEmailConfirm.ToString())
        };

        if (account.Roles?.Any() ?? false)
        {
            foreach (Role role in account.Roles)
            {
                claims.Add(new Claim(CustomJwtClaimTypes.Role, role.Id));
            }
        }

        if (account.Permissions?.Any() ?? false)
        {
            foreach (Permission permission in account.Permissions)
            {
                claims.Add(new Claim(CustomJwtClaimTypes.Permission, permission.Id));
            }
        }

        var token = new JwtSecurityToken(
            _jwtAuthOptions.Issuer,
            _jwtAuthOptions.Audience,
            claims,
            expires: _jwtAuthOptions.TokenLifetime != null
                ? DateTime.Now.Add(_jwtAuthOptions.TokenLifetime.Value)
                : null,
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}