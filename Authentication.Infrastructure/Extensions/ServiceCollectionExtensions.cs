using System.ComponentModel.DataAnnotations;
using Authentication.Infrastructure.Models;
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
        JwtAuthOptions jwtAuthOptions = GetAndValidateJwtAuthOptions(configurationSection);
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

    private static JwtAuthOptions GetAndValidateJwtAuthOptions(IConfiguration configurationSection)
    {
        var jwtAuthOptions = configurationSection.Get<JwtAuthOptions>()!;
        var context = new ValidationContext(jwtAuthOptions);
        Validator.ValidateObject(jwtAuthOptions, context, true);
        return jwtAuthOptions;
    }
}