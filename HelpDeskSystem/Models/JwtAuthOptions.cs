using System.Text;
using Microsoft.IdentityModel.Tokens;

namespace HelpDeskSystem.Models;

public class JwtAuthOptions
{
    public string Issuer { get; set; }
    public string Audience { get; set; }
    public string Secret { get; set; }
    public int TokenMinuteLifetime { get; set; }
    public SymmetricSecurityKey GetSymmetricSecurityKey() => new(Encoding.ASCII.GetBytes(Secret));
}