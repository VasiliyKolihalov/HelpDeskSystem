using System.ComponentModel.DataAnnotations;
using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Infrastructure.Models;

public class JwtAuthOptions
{
    public string? Issuer { get; set; }
    public string? Audience { get; set; }
    public TimeSpan? TokenLifetime { get; set; }
    [Required] public string Secret { get; set; }
    public SymmetricSecurityKey SymmetricSecurityKey => new(Encoding.ASCII.GetBytes(Secret));
}