using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

public static class ConfigurationSectionExtensions
{
    public static string GetRequiredConnectionString(this IConfiguration @this, string name)
    {
        return @this.GetConnectionString(name)
               ?? throw new InvalidOperationException($"Connection string {name} nod found in configuration");
    }
}