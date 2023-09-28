using System.Net.Http.Headers;

namespace Infrastructure.Authentication.Extensions;

public static class HttpRequestHeadersExtensions
{
    public static void AddJwtBearer(this HttpRequestHeaders @this, string jwt)
    {
        @this.Add(name: "Authorization", value: "Bearer " + jwt);
    }
}