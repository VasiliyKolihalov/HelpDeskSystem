using Authentication.Infrastructure.Models;
using Infrastructure.Extensions;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.IdentityModel.Tokens;

namespace Authentication.Infrastructure.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(
        this IServiceCollection @this,
        IConfigurationSection configurationSection,
        Action<JwtBearerOptions>? options = null)
    {
        var jwtAuthOptions = configurationSection.GetAndValidate<JwtAuthOptions>();
        @this.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options ?? (_ =>
            {
                _.RequireHttpsMetadata = false;
                _.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = false,
                    ValidateAudience = false,
                    ValidateLifetime = true,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = jwtAuthOptions.SymmetricSecurityKey
                };
            }));

        return @this;
    }
}