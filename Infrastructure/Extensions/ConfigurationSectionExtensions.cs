using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

public static class ConfigurationSectionExtensions
{
    public static string GetRequiredConnectionString(this IConfiguration @this, string name)
    {
        return @this.GetConnectionString(name)
               ?? throw new InvalidOperationException($"Connection string {name} nod found in configuration");
    }

    public static T GetRequiredValue<T>(this IConfiguration @this, string key)
    {
        return @this.GetValue<T>(key)
               ?? throw new InvalidOperationException($"Value with key {key} not found in configuration");
    }
}