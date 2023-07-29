using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Authentication.Infrastructure.Constants;
using Authentication.Infrastructure.Models;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Infrastructure.Services;

public class JwtService : IJwtService
{
    private readonly JwtAuthOptions _jwtAuthOptions;

    public JwtService(IOptions<JwtAuthOptions> jwtAuthOptions)
    {
        _jwtAuthOptions = jwtAuthOptions.Value;
    }

    public string GenerateJwt<TId>(Account<TId> account) where TId : notnull
    {
        SymmetricSecurityKey securityKey = _jwtAuthOptions.SymmetricSecurityKey;
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(CustomJwtClaimTypes.Id, account.Id.ToString()
                                         ?? throw new InvalidOperationException(
                                             $"Failed to convert {typeof(TId).Name} to string"))
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