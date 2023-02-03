using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using Dapper;
using HelpDeskSystem.Constants;
using HelpDeskSystem.Models;
using HelpDeskSystem.Models.Roles;
using HelpDeskSystem.Models.User;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;

namespace HelpDeskSystem.Services;

public class JwtService : IJwtService
{
    private readonly JwtAuthOptions _jwtAuthOptions;

    public JwtService(IOptions<JwtAuthOptions> jwtAuthOptions)
    {
        _jwtAuthOptions = jwtAuthOptions.Value;
    }

    public string GenerateJwt(User user, IEnumerable<Role>? userRoles = null)
    {
        if (user == null) throw new ArgumentNullException(nameof(user));
        
        SymmetricSecurityKey securityKey = _jwtAuthOptions.GetSymmetricSecurityKey();
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.Id.ToString())
        };
        
        userRoles = userRoles.AsList();
        if (userRoles != null && userRoles.Any())
        {
            IEnumerable<Claim> roleClaims = userRoles.Select(_ => new Claim(ClaimTypes.Role, _.Name));
            IEnumerable<Claim> permissionClaims = userRoles
                .SelectMany(_ => _.Permissions)
                .Distinct()
                .Select(permission => new Claim(CustomClaimTypes.Permission, permission));

            claims = claims.Union(roleClaims).Union(permissionClaims).ToList();
        }

        var token = new JwtSecurityToken(
            _jwtAuthOptions.Issuer,
            _jwtAuthOptions.Audience,
            claims,
            expires: DateTime.Now.AddMinutes(_jwtAuthOptions.TokenMinuteLifetime),
            signingCredentials: credentials);

        return new JwtSecurityTokenHandler().WriteToken(token);
    }
}