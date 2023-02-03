using HelpDeskSystem.Models;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace HelpDeskSystem.Extensions;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddJwtAuthentication(this IServiceCollection @this, JwtAuthOptions jwtAuthOptions)
    {
        @this.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
            .AddJwtBearer(options =>
            {
                options.RequireHttpsMetadata = false;
                options.TokenValidationParameters = new TokenValidationParameters
                {
                    ValidateIssuer = true,
                    ValidIssuer = jwtAuthOptions.Issuer,

                    ValidateAudience = true,
                    ValidAudience = jwtAuthOptions.Audience,
                    ValidateLifetime = true,

                    IssuerSigningKey = jwtAuthOptions.GetSymmetricSecurityKey(),
                    ValidateIssuerSigningKey = true
                };
            });

        return @this;
    }
}