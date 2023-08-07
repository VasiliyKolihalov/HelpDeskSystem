using System.Net;
using Infrastructure.Models;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Polly;
using Polly.Extensions.Http;

namespace Infrastructure.Extensions;

public static class HttpClientBuilderExtensions
{
    public static IHttpClientBuilder AddDefaultRetryPollyHandler(
        this IHttpClientBuilder @this,
        IConfigurationSection configurationSection)
    {
        var pollyOptions = configurationSection.GetAndValidate<PollyOptions>();
        return @this.AddPolicyHandler((context, message) =>
        {
            return HttpPolicyExtensions
                .HandleTransientHttpError()
                .OrResult(_ => !_.IsSuccessStatusCode)
                .WaitAndRetryAsync(
                    retryCount: pollyOptions.RetryCount!.Value,
                    sleepDurationProvider: _ => pollyOptions.RetrySleepDuration!.Value,
                    onRetry: (result, timeSpan) =>
                    {
                        context.GetRequiredService<ILogger<HttpClient>>()
                            .LogError(
                                exception: result.Exception,
                                message: "Error sending {Uri} request. " +
                                         "Status code: {StatusCode}. " +
                                         "Retrying in {TimeSpan} sec",
                                message.RequestUri!.AbsoluteUri, result.Result.StatusCode, timeSpan);
                    });
        });
    }
}