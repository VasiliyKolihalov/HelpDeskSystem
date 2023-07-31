using Authentication.Infrastructure.Extensions;
using Authentication.WebApi.Constants;
using Authentication.WebApi.Models.Http.Users;
using Infrastructure.Models;
using Microsoft.Extensions.Options;
using Polly;

namespace Authentication.WebApi.Clients.Users;

public class UsersClient : IUsersClient
{
    private readonly HttpClient _httpClient;
    private readonly PollyOptions _pollyOptions;
    private readonly ILogger<UsersClient> _logger;

    public UsersClient(IHttpClientFactory factory, IOptions<PollyOptions> pollyOptions, ILogger<UsersClient> logger)
    {
        _httpClient = factory.CreateClient(HttpClientNames.Users);
        _pollyOptions = pollyOptions.Value;
        _logger = logger;
    }

    public async Task<Guid> SendPostRequestAsync(UserCreate userCreate, string jwt)
    {
        _httpClient.DefaultRequestHeaders.AddJwtBearer(jwt);
        HttpResponseMessage result = null!;

        await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _pollyOptions.RetryCount!.Value,
                sleepDurationProvider: _ => _pollyOptions.RetrySleepDuration!.Value,
                onRetry: (exception, timeSpan) =>
                {
                    _logger.LogError(
                        exception: exception,
                        message: "Error sending user create request. Retrying in {TimeSpan} sec",
                        args: timeSpan);
                })
            .ExecuteAsync(async () =>
            {
                result = await _httpClient.PostAsJsonAsync("users", userCreate);
                result.EnsureSuccessStatusCode();
            });

        return await result.Content.ReadFromJsonAsync<Guid>();
    }

    public async Task SendDeleteRequestAsync(Guid userId, string jwt)
    {
        _httpClient.DefaultRequestHeaders.AddJwtBearer(jwt);

        await Policy
            .Handle<Exception>()
            .WaitAndRetryAsync(
                retryCount: _pollyOptions.RetryCount!.Value,
                sleepDurationProvider: _ => _pollyOptions.RetrySleepDuration!.Value,
                onRetry: (exception, timeSpan) =>
                {
                    _logger.LogError(
                        exception: exception,
                        message: "Error sending user delete request. Retrying in {TimeSpan} sec",
                        args: timeSpan);
                })
            .ExecuteAsync(async () =>
            {
                HttpResponseMessage result = await _httpClient.DeleteAsync($"users/{userId}");
                result.EnsureSuccessStatusCode();
            });
    }
}