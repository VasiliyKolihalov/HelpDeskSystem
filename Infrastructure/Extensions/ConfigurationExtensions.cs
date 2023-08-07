using System.ComponentModel.DataAnnotations;
using Microsoft.Extensions.Configuration;

namespace Infrastructure.Extensions;

public static class ConfigurationExtensions
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

    public static T GetAndValidate<T>(this IConfiguration @this)
    {
        T value = @this.Get<T>()
                  ?? throw new InvalidOperationException($"{typeof(T).Name} not found in configuration");
        var context = new ValidationContext(value);
        Validator.ValidateObject(value, context, validateAllProperties: true);
        return value;
    }
}